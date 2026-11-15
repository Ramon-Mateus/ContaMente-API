using ContaMente.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Services.Pluggy
{
    public interface IPluggySyncService
    {
        Task SincronizarTodos();
        Task SincronizarItem(string pluggyItemId);
    }

    // Busca as transações de cartão de crédito no Pluggy e grava em TransacoesPendentes.
    // Roda via Hangfire (job recorrente + disparo pelo webhook / botão "sincronizar").
    public class PluggySyncService : IPluggySyncService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPluggyClient _pluggy;
        private readonly ILogger<PluggySyncService> _logger;

        public PluggySyncService(ApplicationDbContext context, IPluggyClient pluggy, ILogger<PluggySyncService> logger)
        {
            _context = context;
            _pluggy = pluggy;
            _logger = logger;
        }

        private static int DiasImportacaoInicial =>
            int.TryParse(Environment.GetEnvironmentVariable("PLUGGY_DIAS_IMPORTACAO"), out var d) && d > 0 ? d : 60;

        public async Task SincronizarTodos()
        {
            if (!_pluggy.Configurado) return;

            var itens = await _context.PluggyItens.Select(i => i.PluggyItemId).ToListAsync();
            foreach (var itemId in itens)
            {
                try
                {
                    await SincronizarItem(itemId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Pluggy: erro ao sincronizar item {ItemId}", itemId);
                }
            }
        }

        public async Task SincronizarItem(string pluggyItemId)
        {
            var item = await _context.PluggyItens
                .Include(i => i.Contas)
                .FirstOrDefaultAsync(i => i.PluggyItemId == pluggyItemId);

            if (item == null)
            {
                _logger.LogWarning("Pluggy: item {ItemId} não cadastrado, ignorando.", pluggyItemId);
                return;
            }

            var apiItem = await _pluggy.GetItem(pluggyItemId);
            item.NomeConector = apiItem.Connector?.Name ?? item.NomeConector;
            item.Status = apiItem.Status;
            item.ErroUltimaAtualizacao = apiItem.Error?.Message;

            var contasApi = (await _pluggy.GetAccounts(pluggyItemId))
                .Where(a => string.Equals(a.Type, "CREDIT", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var desde = (item.UltimaSincronizacao?.AddDays(-20) ?? DateTime.UtcNow.AddDays(-DiasImportacaoInicial)).Date;

            foreach (var contaApi in contasApi)
            {
                var conta = item.Contas.FirstOrDefault(c => c.PluggyAccountId == contaApi.Id);
                if (conta == null)
                {
                    conta = new PluggyConta { PluggyAccountId = contaApi.Id, ItemId = item.Id };
                    item.Contas.Add(conta);
                }
                conta.Nome = contaApi.MarketingName ?? contaApi.Name ?? "Cartão";
                conta.Numero = contaApi.Number;

                var transacoes = await _pluggy.GetTransactions(contaApi.Id, desde);
                await ImportarTransacoes(item.UserId, conta, transacoes, desde);
            }

            item.UltimaSincronizacao = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private async Task ImportarTransacoes(string userId, PluggyConta conta, List<PluggyApiTransaction> transacoes, DateTime desde)
        {
            // Só saídas (compras). Pagamentos de fatura e estornos vêm como CREDIT.
            // Parcelas futuras já vêm com a data da fatura em que vão cair; entram quando a data chegar.
            var limite = DateTime.UtcNow.Date.AddDays(1);
            var saidas = transacoes
                .Where(t => string.Equals(t.Type, "DEBIT", StringComparison.OrdinalIgnoreCase) && t.Amount != 0)
                .Where(t => t.Date < limite)
                .ToList();

            var existentes = await _context.TransacoesPendentes
                .Where(t => t.UserId == userId && t.PluggyAccountId == conta.PluggyAccountId && t.Data >= desde.AddDays(-10))
                .ToListAsync();

            var porId = existentes.ToDictionary(t => t.PluggyTransactionId);
            var idsRecebidos = saidas.Select(t => t.Id).ToHashSet();

            // Transações "PENDING" no Pluggy podem sumir e reaparecer com outro id quando lançadas.
            // Guardamos as que sumiram para reaproveitar o registro (e o status de aprovação).
            var pendentesSumidas = existentes
                .Where(t => t.Pendente && !idsRecebidos.Contains(t.PluggyTransactionId))
                .ToList();

            var idsJaImportados = await _context.TransacoesPendentes
                .Where(t => t.UserId == userId && idsRecebidos.Contains(t.PluggyTransactionId))
                .Select(t => t.PluggyTransactionId)
                .ToListAsync();
            var jaImportados = idsJaImportados.ToHashSet();

            foreach (var t in saidas)
            {
                var valor = Math.Round(Math.Abs(t.Amount), 2);
                var data = DateTime.SpecifyKind(t.Date, DateTimeKind.Utc);
                var descricao = (t.Merchant?.Name ?? t.Description ?? t.DescriptionRaw ?? "Compra no cartão").Trim();
                var pendente = string.Equals(t.Status, "PENDING", StringComparison.OrdinalIgnoreCase);

                if (!porId.TryGetValue(t.Id, out var registro))
                {
                    if (jaImportados.Contains(t.Id)) continue;

                    registro = pendentesSumidas.FirstOrDefault(p =>
                        Math.Abs(p.Valor - valor) < 0.01 && Math.Abs((p.Data - data).TotalDays) <= 5);

                    if (registro != null)
                    {
                        pendentesSumidas.Remove(registro);
                        registro.PluggyTransactionId = t.Id;
                    }
                    else
                    {
                        registro = new TransacaoPendente
                        {
                            PluggyTransactionId = t.Id,
                            PluggyAccountId = conta.PluggyAccountId,
                            UserId = userId,
                        };
                        _context.TransacoesPendentes.Add(registro);
                    }
                }

                // Depois de aprovada/ignorada, não mexemos mais nos dados.
                if (registro.Status != StatusTransacaoPendente.Pendente && registro.Id != 0)
                {
                    registro.Pendente = pendente;
                    continue;
                }

                registro.NomeConta = conta.Nome;
                registro.FinalCartao = t.CreditCardMetadata?.CardNumber ?? conta.Numero;
                registro.Descricao = descricao.Length > 255 ? descricao[..255] : descricao;
                registro.Valor = valor;
                registro.Data = data;
                registro.CategoriaPluggy = t.Category;
                registro.NumeroParcela = t.CreditCardMetadata?.InstallmentNumber;
                registro.TotalParcelas = t.CreditCardMetadata?.TotalInstallments;
                registro.Pendente = pendente;
            }

            // Pendentes que sumiram do Pluggy e não foram reaproveitadas: removemos se ninguém aprovou ainda.
            foreach (var sumida in pendentesSumidas.Where(p => p.Status == StatusTransacaoPendente.Pendente && p.Data >= desde))
                _context.TransacoesPendentes.Remove(sumida);
        }
    }
}
