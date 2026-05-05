using ContaMente.Contexts;
using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Services
{
    public class ContaReceberService : IContaReceberService
    {
        private readonly ApplicationDbContext _context;
        private readonly IContaReceberRepository _contaReceberRepository;

        public ContaReceberService(
            ApplicationDbContext context,
            IContaReceberRepository contaReceberRepository)
        {
            _context = context;
            _contaReceberRepository = contaReceberRepository;
        }

        public async Task<List<ContaReceberDto>> GetContasReceber(
            int[]? categoriasIds,
            int[]? responsaveisIds,
            DateTime? dataVencimentoInicio,
            DateTime? dataVencimentoFim,
            bool? pago,
            StatusDuplicataEnum? status,
            string userId)
        {
            var query = _contaReceberRepository.GetContasReceber(userId);

            if (categoriasIds != null && categoriasIds.Length > 0)
                query = query.Where(c => c.CategoriasRateio.Any(r => categoriasIds.Contains(r.CategoriaId)));

            if (responsaveisIds != null && responsaveisIds.Length > 0)
                query = query.Where(c => responsaveisIds.Contains(c.ResponsavelId));

            if (dataVencimentoInicio.HasValue)
                query = query.Where(c => c.Vencimento >= dataVencimentoInicio.Value);

            if (dataVencimentoFim.HasValue)
                query = query.Where(c => c.Vencimento <= dataVencimentoFim.Value);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);
            else if (pago.HasValue)
                query = pago.Value
                    ? query.Where(c => c.Status == StatusDuplicataEnum.Paga)
                    : query.Where(c => c.Status != StatusDuplicataEnum.Paga);

            var contas = await query
                .OrderBy(c => c.Vencimento)
                .ThenBy(c => c.NumeroDaParcela)
                .ToListAsync();

            return contas.Select(MapToDto).ToList();
        }

        public async Task<ContaReceberDto?> GetContaReceberById(int id, string userId)
        {
            var conta = await _contaReceberRepository.GetContaReceberById(id, userId);
            return conta == null ? null : MapToDto(conta);
        }

        public async Task<List<ContaReceberDto>> LancarContasReceber(CreateContaReceberDto dto, string userId)
        {
            if (dto.ValorTotal <= 0)
                throw new ArgumentException("O valor total deve ser maior que zero.");

            if (dto.NumeroParcelas <= 0)
                throw new ArgumentException("O número de parcelas deve ser maior que zero.");

            var valorParcela = Math.Round(dto.ValorTotal / dto.NumeroParcelas, 2, MidpointRounding.AwayFromZero);
            var rateiosParcela = await CategoriaRateioHelper.CalcularRateio(
                _context,
                dto.Categorias,
                dto.CategoriaId,
                valorParcela,
                userId,
                entradaEsperada: true);
            var grupoLancamentoId = Guid.NewGuid();
            var resultado = new List<ContaReceberDto>();

            for (int i = 0; i < dto.NumeroParcelas; i++)
            {
                var vencimento = CalcularVencimento(dto.PrimeiroVencimento, dto.TipoIntervalo, dto.Intervalo, i);

                var conta = new ContaReceber
                {
                    GrupoLancamentoId = grupoLancamentoId,
                    UserId = userId,
                    ResponsavelId = dto.ResponsavelId,
                    CategoriaId = rateiosParcela.First().Categoria.Id,
                    Descricao = dto.Descricao,
                    ValorTotal = dto.ValorTotal,
                    ValorParcela = valorParcela,
                    ValorBaixado = 0,
                    ValorRestante = valorParcela,
                    NumeroParcelas = dto.NumeroParcelas,
                    NumeroDaParcela = i + 1,
                    DataEmissao = DateTime.SpecifyKind(dto.DataEmissao, DateTimeKind.Utc),
                    Vencimento = DateTime.SpecifyKind(vencimento, DateTimeKind.Utc),
                    Status = StatusDuplicataEnum.Aberta,
                    CategoriasRateio = rateiosParcela.Select(r => new ContaReceberCategoria
                    {
                        CategoriaId = r.Categoria.Id,
                        Valor = r.Valor,
                        Percentual = r.Percentual
                    }).ToList()
                };

                var criada = await _contaReceberRepository.CreateContaReceber(conta);
                resultado.Add(MapToDto(criada));
            }

            return resultado;
        }

        public async Task<ContaReceberDto?> UpdateContaReceber(int id, UpdateContaReceberDto dto, string userId)
        {
            var conta = await _contaReceberRepository.GetContaReceberById(id, userId);
            if (conta == null)
                return null;

            if (conta.Status != StatusDuplicataEnum.Aberta)
                throw new InvalidOperationException("Não é permitido alterar uma duplicata com status Parcial ou Paga.");

            var categoriasAlteradas = dto.CategoriaId.HasValue || dto.Categorias?.Count > 0;

            if (categoriasAlteradas)
            {
                var rateios = await CategoriaRateioHelper.CalcularRateio(
                    _context,
                    dto.Categorias,
                    dto.CategoriaId ?? conta.CategoriaId,
                    dto.ValorParcela ?? conta.ValorParcela,
                    userId,
                    entradaEsperada: true);

                conta.CategoriaId = rateios.First().Categoria.Id;
                _context.ContasReceberCategorias.RemoveRange(conta.CategoriasRateio);
                conta.CategoriasRateio = rateios.Select(r => new ContaReceberCategoria
                {
                    ContaReceberId = conta.Id,
                    CategoriaId = r.Categoria.Id,
                    Valor = r.Valor,
                    Percentual = r.Percentual
                }).ToList();
            }

            if (dto.ResponsavelId.HasValue) conta.ResponsavelId = dto.ResponsavelId.Value;
            if (dto.Descricao != null) conta.Descricao = dto.Descricao;
            if (dto.ValorTotal.HasValue) conta.ValorTotal = dto.ValorTotal.Value;
            if (dto.ValorParcela.HasValue)
            {
                conta.ValorParcela = dto.ValorParcela.Value;
                conta.ValorRestante = dto.ValorParcela.Value;

                if (!categoriasAlteradas)
                    RecalcularRateioExistente(conta.CategoriasRateio, conta.ValorParcela);
            }
            if (dto.DataEmissao.HasValue) conta.DataEmissao = DateTime.SpecifyKind(dto.DataEmissao.Value, DateTimeKind.Utc);
            if (dto.Vencimento.HasValue) conta.Vencimento = DateTime.SpecifyKind(dto.Vencimento.Value, DateTimeKind.Utc);

            var atualizada = await _contaReceberRepository.UpdateContaReceber(conta);
            return atualizada == null ? null : MapToDto(atualizada);
        }

        public async Task<ContaReceberDto?> BaixarContaReceber(int id, BaixaContaReceberDto dto, string userId)
        {
            var conta = await _contaReceberRepository.GetContaReceberById(id, userId);
            if (conta == null)
                return null;

            if (conta.Status == StatusDuplicataEnum.Paga)
                throw new InvalidOperationException("A duplicata já está paga.");

            if (dto.Valor <= 0)
                throw new ArgumentException("O valor da baixa deve ser maior que zero.");

            if (dto.Valor > conta.ValorRestante)
                throw new ArgumentException("O valor da baixa não pode ser maior que o valor restante.");

            var rateiosBaixa = CriarRateioMovimentacaoDaBaixa(conta.CategoriasRateio, dto.Valor);

            var movimentacao = new Movimentacao
            {
                Valor = (double)dto.Valor,
                Descricao = dto.Descricao ?? conta.Descricao,
                Data = dto.Data.ToUniversalTime(),
                Fixa = false,
                CategoriaId = rateiosBaixa.First().CategoriaId,
                TipoPagamento = (TipoPagamentoEnum)dto.TipoPagamentoId,
                ResponsavelId = dto.ResponsavelId ?? conta.ResponsavelId,
                CartaoId = dto.CartaoId,
                CategoriasRateio = rateiosBaixa
            };

            _context.Movimentacoes.Add(movimentacao);
            await _context.SaveChangesAsync();

            _context.ContasReceberMovimentacoes.Add(new ContaReceberMovimentacao
            {
                ContaReceberId = conta.Id,
                MovimentacaoId = movimentacao.Id,
                ValorBaixado = dto.Valor,
                DataBaixa = dto.Data.ToUniversalTime()
            });

            RecalcularStatus(conta, conta.ValorBaixado + dto.Valor);
            await _context.SaveChangesAsync();

            return MapToDto(conta);
        }

        public async Task<bool> DeleteContaReceber(int id, string userId)
        {
            var conta = await _contaReceberRepository.GetContaReceberById(id, userId);
            if (conta == null)
                return false;

            if (conta.Movimentacoes.Count > 0)
                throw new InvalidOperationException("Não é permitido excluir uma duplicata com movimentações vinculadas.");

            return await _contaReceberRepository.DeleteContaReceber(conta);
        }

        private static void RecalcularStatus(ContaReceber conta, decimal valorBaixado)
        {
            conta.ValorBaixado = Math.Min(valorBaixado, conta.ValorParcela);
            conta.ValorRestante = Math.Max(conta.ValorParcela - conta.ValorBaixado, 0);

            if (conta.ValorBaixado <= 0)
            {
                conta.Status = StatusDuplicataEnum.Aberta;
                conta.DataPagamento = null;
            }
            else if (conta.ValorRestante > 0)
            {
                conta.Status = StatusDuplicataEnum.Parcial;
                conta.DataPagamento = null;
            }
            else
            {
                conta.Status = StatusDuplicataEnum.Paga;
                conta.DataPagamento = DateTime.UtcNow;
            }
        }

        private static DateTime CalcularVencimento(DateTime primeiroVencimento, TipoIntervaloEnum tipoIntervalo, int intervalo, int indiceParcela)
        {
            if (indiceParcela == 0)
                return primeiroVencimento;

            if (tipoIntervalo == TipoIntervaloEnum.Dias)
                return primeiroVencimento.AddDays((double)intervalo * indiceParcela);

            return AjustarUltimoDiaMes(primeiroVencimento, intervalo * indiceParcela);
        }

        private static DateTime AjustarUltimoDiaMes(DateTime baseDate, int meses)
        {
            var resultado = baseDate.AddMonths(meses);
            var diaOriginal = baseDate.Day;
            var ultimoDiaMes = DateTime.DaysInMonth(resultado.Year, resultado.Month);
            var dia = diaOriginal > ultimoDiaMes ? ultimoDiaMes : diaOriginal;
            return new DateTime(resultado.Year, resultado.Month, dia, baseDate.Hour, baseDate.Minute, baseDate.Second, baseDate.Kind);
        }

        private static ContaReceberDto MapToDto(ContaReceber c)
        {
            return new ContaReceberDto
            {
                Id = c.Id,
                GrupoLancamentoId = c.GrupoLancamentoId,
                Responsavel = new ResponsavelDto { Id = c.Responsavel.Id, Nome = c.Responsavel.Nome },
                Categoria = new CategoriaDto { Id = c.Categoria.Id, Nome = c.Categoria.Nome, Entrada = c.Categoria.Entrada },
                Categorias = CategoriaRateioHelper.MapearDetalhes(
                    c.CategoriasRateio,
                    r => r.Categoria,
                    r => r.Valor,
                    r => r.Percentual),
                Descricao = c.Descricao,
                ValorTotal = c.ValorTotal,
                ValorParcela = c.ValorParcela,
                ValorBaixado = c.ValorBaixado,
                ValorRestante = c.ValorRestante,
                NumeroParcelas = c.NumeroParcelas,
                NumeroDaParcela = c.NumeroDaParcela,
                DataEmissao = c.DataEmissao,
                Vencimento = c.Vencimento,
                Status = c.Status,
                Pago = c.Status == StatusDuplicataEnum.Paga,
                DataPagamento = c.DataPagamento,
            };
        }

        private static List<MovimentacaoCategoria> CriarRateioMovimentacaoDaBaixa(
            List<ContaReceberCategoria> rateiosDuplicata,
            decimal valorBaixa)
        {
            var resultado = new List<MovimentacaoCategoria>();
            decimal acumulado = 0;

            for (var i = 0; i < rateiosDuplicata.Count; i++)
            {
                var rateio = rateiosDuplicata[i];
                var valor = i == rateiosDuplicata.Count - 1
                    ? valorBaixa - acumulado
                    : Math.Round(valorBaixa * rateio.Percentual / 100, 2, MidpointRounding.AwayFromZero);

                acumulado += valor;
                resultado.Add(new MovimentacaoCategoria
                {
                    CategoriaId = rateio.CategoriaId,
                    Valor = valor,
                    Percentual = rateio.Percentual
                });
            }

            return resultado;
        }

        private static void RecalcularRateioExistente(List<ContaReceberCategoria> rateios, decimal novoTotal)
        {
            decimal acumulado = 0;

            for (var i = 0; i < rateios.Count; i++)
            {
                var valor = i == rateios.Count - 1
                    ? novoTotal - acumulado
                    : Math.Round(novoTotal * rateios[i].Percentual / 100, 2, MidpointRounding.AwayFromZero);

                rateios[i].Valor = valor;
                acumulado += valor;
            }
        }
    }
}
