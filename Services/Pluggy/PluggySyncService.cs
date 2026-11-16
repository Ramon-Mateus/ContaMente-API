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
                var inicioImportacao = item.CriadoEm.AddDays(-DiasImportacaoInicial).Date;
                await ImportarAVista(item.UserId, conta, transacoes, desde);
                await ImportarParceladas(item.UserId, conta, transacoes, inicioImportacao);
            }

            item.UltimaSincronizacao = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private static bool EhSaida(PluggyApiTransaction t) =>
            string.Equals(t.Type, "DEBIT", StringComparison.OrdinalIgnoreCase) && t.Amount != 0;

        private static bool EhParcelada(PluggyApiTransaction t) => (t.CreditCardMetadata?.TotalInstallments ?? 1) > 1;

        private static string Descricao(PluggyApiTransaction t)
        {
            var d = (t.Merchant?.Name ?? t.Description ?? t.DescriptionRaw ?? "Compra no cartão").Trim();
            return d.Length > 255 ? d[..255] : d;
        }

        // Compras à vista: uma transação do Pluggy = um registro.
        private async Task ImportarAVista(string userId, PluggyConta conta, List<PluggyApiTransaction> transacoes, DateTime desde)
        {
            // Só saídas (compras). Pagamentos de fatura e estornos vêm como CREDIT.
            var limite = DateTime.UtcNow.Date.AddDays(1);
            var saidas = transacoes
                .Where(t => EhSaida(t) && !EhParcelada(t) && t.Date < limite)
                .ToList();

            var existentes = await _context.TransacoesPendentes
                .Where(t => t.UserId == userId && t.PluggyAccountId == conta.PluggyAccountId && t.ChaveCompra == null && t.Data >= desde.AddDays(-10))
                .ToListAsync();

            var porId = existentes.ToDictionary(t => t.PluggyTransactionId);
            var idsRecebidos = saidas.Select(t => t.Id).ToHashSet();

            // Transações "PENDING" no Pluggy podem sumir e reaparecer com outro id quando lançadas.
            // Guardamos as que sumiram para reaproveitar o registro (e o status de aprovação).
            var pendentesSumidas = existentes
                .Where(t => t.Pendente && !idsRecebidos.Contains(t.PluggyTransactionId))
                .ToList();

            var jaImportados = (await _context.TransacoesPendentes
                .Where(t => t.UserId == userId && idsRecebidos.Contains(t.PluggyTransactionId))
                .Select(t => t.PluggyTransactionId)
                .ToListAsync()).ToHashSet();

            foreach (var t in saidas)
            {
                var valor = Math.Round(Math.Abs(t.Amount), 2);
                var data = DateTime.SpecifyKind(t.Date, DateTimeKind.Utc);
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
                registro.Descricao = Descricao(t);
                registro.Valor = valor;
                registro.Data = data;
                registro.CategoriaPluggy = t.Category;
                registro.Pendente = pendente;
            }

            // Pendentes que sumiram do Pluggy e não foram reaproveitadas: removemos se ninguém aprovou ainda.
            foreach (var sumida in pendentesSumidas.Where(p => p.Status == StatusTransacaoPendente.Pendente && p.Data >= desde))
                _context.TransacoesPendentes.Remove(sumida);
        }

        // Compras parceladas: o Pluggy manda cada parcela como uma transação (inclusive as futuras).
        // Agrupamos pela compra e criamos um único registro, aprovado como Parcela no sistema.
        // As parcelas que chegarem nos meses seguintes caem na mesma chave e são ignoradas.
        private async Task ImportarParceladas(string userId, PluggyConta conta, List<PluggyApiTransaction> transacoes, DateTime inicioImportacao)
        {
            var limite = DateTime.UtcNow.Date.AddDays(1);

            var compras = transacoes
                .Where(t => EhSaida(t) && EhParcelada(t))
                .GroupBy(t => ChaveCompra(conta.PluggyAccountId, t))
                .ToList();

            // Registros de parcelas individuais criados antes do agrupamento existir: descartamos os não aprovados.
            var legados = await _context.TransacoesPendentes
                .Where(t => t.UserId == userId && t.PluggyAccountId == conta.PluggyAccountId && t.ChaveCompra == null
                            && t.TotalParcelas > 1 && t.Status == StatusTransacaoPendente.Pendente)
                .ToListAsync();
            _context.TransacoesPendentes.RemoveRange(legados);

            if (compras.Count == 0) return;

            var chaves = compras.Select(g => g.Key).ToList();
            var existentes = await _context.TransacoesPendentes
                .Where(t => t.UserId == userId && t.ChaveCompra != null && chaves.Contains(t.ChaveCompra))
                .ToDictionaryAsync(t => t.ChaveCompra!);

            // Registros que já usam o id de alguma parcela (PluggyTransactionId é único por usuário):
            // parcelas legadas aprovadas/ignoradas ou compras agrupadas cuja chave mudou.
            var idsLegadosRemovidos = legados.Select(l => l.Id).ToHashSet();
            var idsParcelas = compras.SelectMany(g => g.Select(t => t.Id)).Distinct().ToList();
            var porTransacao = (await _context.TransacoesPendentes
                    .Where(t => t.UserId == userId && idsParcelas.Contains(t.PluggyTransactionId))
                    .ToListAsync())
                .Where(t => !idsLegadosRemovidos.Contains(t.Id))
                .ToDictionary(t => t.PluggyTransactionId);

            foreach (var compra in compras)
            {
                var parcelas = compra.OrderBy(t => t.CreditCardMetadata?.InstallmentNumber ?? 0).ThenBy(t => t.Date).ToList();
                var primeira = parcelas[0];
                var meta = primeira.CreditCardMetadata!;
                var totalParcelas = meta.TotalInstallments!.Value;
                var valorParcela = Math.Round(Math.Abs(primeira.Amount), 2);
                var dataCompra = DataCompra(primeira);

                // Só compras feitas a partir do início da importação (evita trazer parcelamentos antigos já lançados à mão).
                if (dataCompra < inicioImportacao || dataCompra >= limite) continue;

                var jaRegistradas = parcelas
                    .Select(p => porTransacao.GetValueOrDefault(p.Id))
                    .Where(r => r != null)
                    .ToList();

                // Alguma parcela já foi aprovada/ignorada individualmente (antes do agrupamento): compra já tratada.
                if (jaRegistradas.Any(r => r!.ChaveCompra == null)) continue;

                if (!existentes.TryGetValue(compra.Key, out var registro))
                    registro = jaRegistradas.FirstOrDefault(r => r!.ChaveCompra != null);

                if (registro == null)
                {
                    registro = new TransacaoPendente
                    {
                        PluggyTransactionId = primeira.Id,
                        PluggyAccountId = conta.PluggyAccountId,
                        UserId = userId,
                        ChaveCompra = compra.Key,
                    };
                    _context.TransacoesPendentes.Add(registro);
                    existentes[compra.Key] = registro;
                }
                else if (registro.Status != StatusTransacaoPendente.Pendente)
                {
                    continue;
                }

                // Valor total: soma das parcelas quando vieram todas; senão o total informado; senão parcela × n.
                var numeros = parcelas.Select(p => p.CreditCardMetadata?.InstallmentNumber).Where(n => n.HasValue).Distinct().Count();
                var valorTotal = numeros == totalParcelas
                    ? Math.Round(parcelas.GroupBy(p => p.CreditCardMetadata!.InstallmentNumber).Sum(g => Math.Abs(g.First().Amount)), 2)
                    : meta.TotalAmount is > 0
                        ? Math.Round(Math.Abs(meta.TotalAmount.Value), 2)
                        : Math.Round(valorParcela * totalParcelas, 2);

                registro.NomeConta = conta.Nome;
                registro.FinalCartao = meta.CardNumber ?? conta.Numero;
                registro.Descricao = Descricao(primeira);
                registro.Valor = valorParcela;
                registro.ValorTotal = valorTotal;
                registro.Data = dataCompra;
                registro.CategoriaPluggy = primeira.Category;
                registro.NumeroParcela = meta.InstallmentNumber;
                registro.TotalParcelas = totalParcelas;
                registro.Pendente = false;
            }
        }

        private static DateTime DataCompra(PluggyApiTransaction t)
        {
            var meta = t.CreditCardMetadata!;
            if (meta.PurchaseDate.HasValue)
                return DateTime.SpecifyKind(meta.PurchaseDate.Value, DateTimeKind.Utc);

            // Sem data da compra: estima pela data da parcela recuando (n - 1) meses.
            var n = meta.InstallmentNumber ?? 1;
            return DateTime.SpecifyKind(t.Date.AddMonths(-(n - 1)).Date, DateTimeKind.Utc);
        }

        // Identifica a compra: mesma conta, mesma data/hora da compra e mesmo número de parcelas.
        // (O valor fica de fora: a última parcela costuma ter centavos de diferença.)
        private static string ChaveCompra(string accountId, PluggyApiTransaction t)
        {
            var meta = t.CreditCardMetadata!;
            if (meta.PurchaseDate.HasValue)
                return $"{accountId}|{meta.PurchaseDate.Value.ToUniversalTime():yyyy-MM-ddTHH:mm:ss.fff}|{meta.TotalInstallments}";

            var descricao = new string(Descricao(t).ToLowerInvariant().Where(char.IsLetter).ToArray());
            return $"{accountId}|{DataCompra(t):yyyy-MM}|{meta.TotalInstallments}|{descricao}";
        }
    }
}
