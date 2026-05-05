using ContaMente.DTOs;
using ContaMente.Contexts;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Services
{
    public class MovimentacaoService : IMovimentacaoService
    {
        #region Services
        private readonly IRecorrenciaService _recorrenciaService;
        private readonly IUserConfigurationService _userConfigurationService;
        private readonly IMovimentacaoParcelaService _movimentacaoParcelaService;
        #endregion
        
        #region Repositories
        private readonly IMovimentacaoRepository _movimentacaoRepository;
        private readonly ICartaoRepository _cartaoRepository;
        private readonly ApplicationDbContext _context;
        #endregion
        
        #region Variaveis
        private IQueryable<Movimentacao>? _queryableMovimentacao;
        #endregion
        
        public MovimentacaoService(
            IMovimentacaoRepository movimentacaoRepository,
            IRecorrenciaService recorrenciaService,
            IMovimentacaoParcelaService movimentacaoParcelaService,
            IUserConfigurationService userConfigurationService,
            ICartaoRepository cartaoRepository,
            ApplicationDbContext context)
        {
            _movimentacaoRepository = movimentacaoRepository;
            _recorrenciaService = recorrenciaService;
            _movimentacaoParcelaService = movimentacaoParcelaService;
            _userConfigurationService = userConfigurationService;
            _cartaoRepository = cartaoRepository;
            _context = context;
        }

        public async Task<Dictionary<DateTime, List<MovimentacaoDto>>> GetMovimentacoes(
            int? mes,
            int? ano,
            string userId,
            bool? entrada,
            List<int> categoriasIds,
            List<int> tiposPagamentoIds,
            List<int> responsaveisIds,
            List<int> cartoesIds)
        {
            _queryableMovimentacao = _movimentacaoRepository.GetMovimentacoes(userId);
            
            await ListagemPorFatura(mes, ano, userId, entrada);

            CategoriasETiposPagamentoFiltro(categoriasIds, tiposPagamentoIds);

            ResponsaveisFiltro(responsaveisIds);

            CartoesFiltro(cartoesIds);

            var movimentacoes = await _queryableMovimentacao
                .OrderByDescending(m => m.Data)
                .ToListAsync();

            var movimentacoesPorDia = MapeiaDiasFiscais(movimentacoes);

            return movimentacoesPorDia;
        }

        private async Task ListagemPorFatura(
            int? mes,
            int? ano,
            string userId,
            bool? entrada)
        {
            var userConfig = await _userConfigurationService.GetUserConfiguration(userId);

            if (userConfig!.ListagemPorFatura && mes.HasValue && ano.HasValue)
            {
                // Buscar todos os cartões do usuário
                var cartoes = await _cartaoRepository.GetCartoes(userId);

                // Movimentações sem cartão (cartaoId is null) do mês completo
                var queryMovsSemCartao = _queryableMovimentacao!.Where(m => m.CartaoId == null &&
                                                                            m.Data.Month == mes.Value &&
                                                                            m.Data.Year == ano.Value);

                if (entrada.HasValue)
                    queryMovsSemCartao = queryMovsSemCartao.Where(m => m.CategoriasRateio.Any(c => c.Categoria.Entrada == entrada.Value));

                // Para cada cartão, aplicar a lógica de fatura
                var queriesMovsComCartao = new List<IQueryable<Movimentacao>>();

                foreach (var cartao in cartoes)
                {
                    var diaFechamento = cartao.DiaFechamento;

                    // Data de início da fatura (dia do fechamento do mês ANTERIOR)
                    DateTime dataInicioFatura;

                    if (mes.Value == 1)
                    {
                        dataInicioFatura = new DateTime(ano.Value - 1, 12, diaFechamento, 0, 0, 0, DateTimeKind.Utc);
                    }
                    else
                    {
                        dataInicioFatura = new DateTime(ano.Value, mes.Value - 1, diaFechamento, 0, 0, 0, DateTimeKind.Utc);
                    }

                    // Data de fim da fatura (um dia antes do fechamento do mês solicitado)
                    var dataFimFatura = new DateTime(ano.Value, mes.Value, diaFechamento, 0, 0, 0, DateTimeKind.Utc);

                    // Buscar movimentações deste cartão que estão dentro do período da fatura
                    var queryCartao = _queryableMovimentacao!.Where(m => m.CartaoId == cartao.Id &&
                                                                         m.Data >= dataInicioFatura &&
                                                                         m.Data < dataFimFatura);

                    if (entrada.HasValue)
                        queryCartao = queryCartao.Where(m => m.CategoriasRateio.Any(c => c.Categoria.Entrada == entrada.Value));

                    queriesMovsComCartao.Add(queryCartao);
                }

                // Unir todas as queries
                if (queriesMovsComCartao.Any())
                {
                    var queryUnificada = queryMovsSemCartao;
                    foreach (var queryCartao in queriesMovsComCartao)
                    {
                        queryUnificada = queryUnificada.Union(queryCartao);
                    }
                    _queryableMovimentacao = queryUnificada;
                }
                else
                {
                    _queryableMovimentacao = queryMovsSemCartao;
                }
            }
            else
            {
                if (mes.HasValue)
                    _queryableMovimentacao = _queryableMovimentacao!.Where(m => m.Data.Month == mes.Value);

                if (ano.HasValue)
                    _queryableMovimentacao = _queryableMovimentacao!.Where(m => m.Data.Year == ano.Value);
                    
                if (entrada.HasValue)
                    _queryableMovimentacao = _queryableMovimentacao!.Where(m => m.CategoriasRateio.Any(c => c.Categoria.Entrada == entrada.Value));
            }
        }

        private void CategoriasETiposPagamentoFiltro(
            List<int> categoriasIds,
            List<int> tiposPagamentoIds)
        {
            if (categoriasIds.Count != 0)
                _queryableMovimentacao = _queryableMovimentacao!.Where(m => m.CategoriasRateio.Any(c => categoriasIds.Contains(c.CategoriaId)));

            if (tiposPagamentoIds.Count != 0)
            {
                var tiposPagamento = tiposPagamentoIds
                    .Select(id => (TipoPagamentoEnum)id)
                    .ToList();

                _queryableMovimentacao = _queryableMovimentacao!.Where(m => tiposPagamento.Contains(m.TipoPagamento));
            }
        }

        private void ResponsaveisFiltro(
            List<int> responsaveisIds)
        {
            //Filtros de responsáveis
            bool filtrarResponsavelPorNulo = responsaveisIds.Contains(0);
            var outrosResponsaveisIds = responsaveisIds.Where(id => id != 0).ToList();

            if (filtrarResponsavelPorNulo && outrosResponsaveisIds.Count > 0)
            {
                _queryableMovimentacao = _queryableMovimentacao!.Where(m => m.ResponsavelId == null ||
                                                                           (m.ResponsavelId.HasValue && outrosResponsaveisIds.Contains(m.ResponsavelId.Value)));
            }
            else if (filtrarResponsavelPorNulo)
            {
                _queryableMovimentacao = _queryableMovimentacao!.Where(m => m.ResponsavelId == null);
            }
            else if (outrosResponsaveisIds.Count > 0)
            {
                _queryableMovimentacao = _queryableMovimentacao!.Where(m => m.ResponsavelId.HasValue && outrosResponsaveisIds.Contains(m.ResponsavelId.Value));
            }
            else if (responsaveisIds.Count > 0)
            {
                _queryableMovimentacao = _queryableMovimentacao!.Where(m => m.ResponsavelId.HasValue && responsaveisIds.Contains(m.ResponsavelId.Value));
            }
        }

        private void CartoesFiltro(
            List<int> cartoesIds)
        {
            //Filtros de cartões
            bool filtrarCartaoPorNulo = cartoesIds.Contains(0);
            var outrosCartoesIds = cartoesIds.Where(id => id != 0).ToList();

            if (filtrarCartaoPorNulo && outrosCartoesIds.Count > 0)
            {
                _queryableMovimentacao = _queryableMovimentacao!.Where(m => m.CartaoId == null ||
                                                                           (m.CartaoId.HasValue && outrosCartoesIds.Contains(m.CartaoId.Value)));
            }
            else if (filtrarCartaoPorNulo)
            {
                _queryableMovimentacao = _queryableMovimentacao!.Where(m => m.CartaoId == null);
            }
            else if (outrosCartoesIds.Count > 0)
            {
                _queryableMovimentacao = _queryableMovimentacao!.Where(m => m.CartaoId.HasValue && outrosCartoesIds.Contains(m.CartaoId.Value));
            }
            else if (cartoesIds.Count > 0)
            {
                _queryableMovimentacao = _queryableMovimentacao!.Where(m => m.CartaoId.HasValue && cartoesIds.Contains(m.CartaoId.Value));
            }
        }

        private Dictionary<DateTime,List<MovimentacaoDto>> MapeiaDiasFiscais(
            List<Movimentacao> movimentacoes)
        {
            return movimentacoes
                .GroupBy(m => m.Data.Date.AddDays(1))
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(MapToMovimentacaoDto).ToList()
                );
        }
        
        public async Task<MovimentacaoDto?> GetMovimentacaoById(int id, string userId)
        {
            var mov = await _movimentacaoRepository.GetMovimentacaoById(id, userId);

            if (mov == null)
                return null;

            return MapToMovimentacaoDto(mov);
        }

        private static MovimentacaoDto MapToMovimentacaoDto(Movimentacao m)
        {
            return new MovimentacaoDto
            {
                Id = m.Id,
                Valor = m.Valor,
                Data = m.Data,
                Descricao = m.Descricao,
                Fixa = m.Fixa,
                NumeroParcela = m.NumeroParcela,
                TipoPagamento = m.TipoPagamento != 0
                    ? new TipoPagamentoDto
                    {
                        Id = (int)m.TipoPagamento,
                        Nome = ((TipoPagamentoEnum)m.TipoPagamento).GetDisplayName()
                    }
                    : null,
                Responsavel = m.Responsavel != null
                    ? new ResponsavelDto
                    {
                        Id = m.Responsavel.Id,
                        Nome = m.Responsavel.Nome
                    }
                    : null,
                Categoria = m.Categoria != null
                    ? new CategoriaDto
                    {
                        Id = m.Categoria.Id,
                        Nome = m.Categoria.Nome,
                        Entrada = m.Categoria.Entrada
                    }
                    : null,
                Categorias = CategoriaRateioHelper.MapearDetalhes(
                    m.CategoriasRateio,
                    r => r.Categoria,
                    r => r.Valor,
                    r => r.Percentual),
                Recorrencia = m.Recorrencia != null
                    ? new RecorrenciaDto
                    {
                        Id = m.Recorrencia.Id,
                        DataInicio = m.Recorrencia.DataInicio,
                        DataFim = m.Recorrencia.DataFim
                    }
                    : null,
                Parcela = m.Parcela != null
                    ? new ParcelaDto
                    {
                        Id = m.Parcela.Id,
                        ValorTotal = m.Parcela.ValorTotal,
                        NumeroParcelas = m.Parcela.NumeroParcelas,
                        ValorParcela = m.Parcela.ValorParcela,
                        DataInicio = m.Parcela.DataInicio,
                        DataFim = m.Parcela.DataFim
                    }
                    : null,
                Cartao = m.Cartao != null
                    ? new CartaoDto
                    {
                        Id = m.Cartao.Id,
                        Apelido = m.Cartao.Apelido,
                        DiaFechamento = m.Cartao.DiaFechamento
                    }
                    : null
            };
        }

        public async Task<Movimentacao> CreateMovimentacao(CreateMovimentacaoDto createMovimentacaoDto, string userId)
        {
            var entradaEsperada = await ObterEntradaEsperada(createMovimentacaoDto.Categorias, createMovimentacaoDto.CategoriaId, userId);
            var rateios = await CategoriaRateioHelper.CalcularRateio(
                _context,
                createMovimentacaoDto.Categorias,
                createMovimentacaoDto.CategoriaId,
                (decimal)createMovimentacaoDto.Valor,
                userId,
                entradaEsperada);
            var categoriaPrincipal = rateios.First().Categoria;

            var movimentacao = new Movimentacao
            {
                Valor = createMovimentacaoDto.Valor,
                Descricao = createMovimentacaoDto.Descricao,
                Data = createMovimentacaoDto.Data.ToUniversalTime(),
                Fixa = createMovimentacaoDto.Fixa,
                CategoriaId = categoriaPrincipal.Id,
                TipoPagamento = (TipoPagamentoEnum)createMovimentacaoDto.TipoPagamentoId,
                ParcelaId = createMovimentacaoDto.ParcelaId,
                NumeroParcela = createMovimentacaoDto.NumeroParcela,
                ResponsavelId = createMovimentacaoDto.ResponsavelId,
                CartaoId = createMovimentacaoDto.CartaoId,
                CategoriasRateio = rateios.Select(r => new MovimentacaoCategoria
                {
                    CategoriaId = r.Categoria.Id,
                    Valor = r.Valor,
                    Percentual = r.Percentual
                }).ToList()
            };

            var createdMovimentacao = await _movimentacaoRepository.CreateMovimentacao(movimentacao);

            if (createMovimentacaoDto.Fixa)
            {
                var recorrencia = new Recorrencia
                {
                    DataInicio = DateTime.UtcNow,
                };

                recorrencia.Movimentacoes.Add(createdMovimentacao);
                await _recorrenciaService.CreateRecorrencia(recorrencia);

                RecurringJob.AddOrUpdate(
                    $"recorrencia_{recorrencia.Id}",
                    () => CriarMovimentacaoRecorrente(recorrencia.Id),
                    "0 0 1 * *"); // A cada 10 segundos: "*/10 * * * * *" // Todo dia primeiro às meia noite: "0 0 1 * *"
            }

            return createdMovimentacao;
        }

        public async Task CriarMovimentacaoRecorrente(int recorrenciaId)
        {
            var recorrencia = await _recorrenciaService.GetRecorrenciaById(recorrenciaId);

            if (recorrencia != null && (recorrencia.DataFim == null || recorrencia.DataFim > DateTime.UtcNow))
            {
                var movimentacaoOriginal = recorrencia.Movimentacoes.First();

                var novaMovimentacao = new Movimentacao
                {
                    Valor = movimentacaoOriginal.Valor,
                    Descricao = movimentacaoOriginal.Descricao,
                    Data = DateTime.UtcNow,
                    Fixa = true,
                    CategoriaId = movimentacaoOriginal.CategoriaId,
                    TipoPagamento = movimentacaoOriginal.TipoPagamento,
                    RecorrenciaId = recorrencia.Id,
                    ResponsavelId = movimentacaoOriginal.ResponsavelId,
                    CartaoId = movimentacaoOriginal.CartaoId,
                    CategoriasRateio = movimentacaoOriginal.CategoriasRateio.Select(r => new MovimentacaoCategoria
                    {
                        CategoriaId = r.CategoriaId,
                        Valor = r.Valor,
                        Percentual = r.Percentual
                    }).ToList()
                };

                await _movimentacaoRepository.CreateMovimentacao(novaMovimentacao);
            }
        }

        private async Task<bool> ObterEntradaEsperada(List<CategoriaRateioDto>? categorias, int? categoriaId, string userId)
        {
            var id = categorias?.FirstOrDefault()?.CategoriaId ?? categoriaId;
            if (!id.HasValue)
                throw new ArgumentException("Informe pelo menos uma categoria.");

            var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id.Value && c.UserId == userId);
            if (categoria == null)
                throw new KeyNotFoundException($"Categoria com ID {id.Value} não encontrada.");

            return categoria.Entrada;
        }

        public async Task<Movimentacao?> UpdateMovimentacao(int id, UpdateMovimentacaoDto updateMovimentacaoDto, string userId)
        {
            var movimentacao = await _movimentacaoRepository.GetMovimentacaoById(id, userId);

            if (movimentacao == null)
            {
                return null;
            }

            var novoValor = updateMovimentacaoDto.Valor ?? movimentacao.Valor;
            await ValidarEAtualizarBaixasVinculadas(movimentacao, (decimal)novoValor);

            if (updateMovimentacaoDto.Valor.HasValue)
            {
                movimentacao.Valor = updateMovimentacaoDto.Valor.Value;
                if (updateMovimentacaoDto.CategoriaId == null && updateMovimentacaoDto.Categorias == null)
                    RecalcularRateioExistente(movimentacao.CategoriasRateio, (decimal)movimentacao.Valor);
            }

            if (updateMovimentacaoDto.Data.HasValue)
            {
                movimentacao.Data = updateMovimentacaoDto.Data.Value;
            }

            if (!string.IsNullOrEmpty(updateMovimentacaoDto.Descricao))
            {
                movimentacao.Descricao = updateMovimentacaoDto.Descricao;
            }

            if (updateMovimentacaoDto.CategoriaId.HasValue || updateMovimentacaoDto.Categorias?.Count > 0)
            {
                if (movimentacao.ContasPagar.Count > 0 || movimentacao.ContasReceber.Count > 0)
                    throw new InvalidOperationException("Não é permitido alterar categorias de movimentação originada de baixa de duplicata.");

                var rateios = await CategoriaRateioHelper.CalcularRateio(
                    _context,
                    updateMovimentacaoDto.Categorias,
                    updateMovimentacaoDto.CategoriaId ?? movimentacao.CategoriaId,
                    (decimal)(updateMovimentacaoDto.Valor ?? movimentacao.Valor),
                    userId,
                    entradaEsperada: movimentacao.Categoria!.Entrada);

                movimentacao.CategoriaId = rateios.First().Categoria.Id;
                _context.MovimentacoesCategorias.RemoveRange(movimentacao.CategoriasRateio);
                movimentacao.CategoriasRateio = rateios.Select(r => new MovimentacaoCategoria
                {
                    MovimentacaoId = movimentacao.Id,
                    CategoriaId = r.Categoria.Id,
                    Valor = r.Valor,
                    Percentual = r.Percentual
                }).ToList();
            }

            if (updateMovimentacaoDto.TipoPagamentoId.HasValue)
            {
                movimentacao.TipoPagamento = (TipoPagamentoEnum)updateMovimentacaoDto.TipoPagamentoId.Value;
            }

            movimentacao.ResponsavelId = updateMovimentacaoDto.ResponsavelId;
            movimentacao.CartaoId      = updateMovimentacaoDto.CartaoId;

            if (updateMovimentacaoDto.Fixa.HasValue && updateMovimentacaoDto.Fixa.Value != movimentacao.Fixa)
            {
                if (!updateMovimentacaoDto.Fixa.Value)
                {
                    await _recorrenciaService.CancelarRecorrencia(movimentacao.RecorrenciaId!.Value, userId);
                    movimentacao.Fixa = false;
                    movimentacao.RecorrenciaId = null;
                }
                else
                {
                    var recorrencia = new Recorrencia
                    {
                        DataInicio = DateTime.UtcNow,
                    };

                    movimentacao.Fixa = true;

                    recorrencia.Movimentacoes.Add(movimentacao);
                    await _recorrenciaService.CreateRecorrencia(recorrencia);

                    RecurringJob.AddOrUpdate(
                        $"recorrencia_{recorrencia.Id}",
                        () => CriarMovimentacaoRecorrente(recorrencia.Id),
                        "0 0 1 * *"); // A cada 10 segundos: "*/10 * * * * *" // Todo dia primeiro às meia noite: "0 0 1 * *"
                }
            }
            
            var movimentacaoAtualizada = await _movimentacaoRepository.UpdateMovimentacao(movimentacao);

            return movimentacaoAtualizada;
        }

        public async Task<bool> DeleteMovimentacao(int id, string userId)
        {
            var movimentacao = await _movimentacaoRepository.GetMovimentacaoById(id, userId);

            if (movimentacao == null)
            {
                return false;
            }

            if (movimentacao.Parcela != null)
            {
                return await _movimentacaoParcelaService.DeleteParcela(movimentacao.Parcela.Id, userId);
            }

            await RemoverBaixasVinculadas(movimentacao);

            return await _movimentacaoRepository.DeleteMovimentacao(movimentacao);
        }

        private async Task ValidarEAtualizarBaixasVinculadas(Movimentacao movimentacao, decimal novoValor)
        {
            var vinculosPagar = await _context.ContasPagarMovimentacoes
                .Include(v => v.ContaPagar)
                .Where(v => v.MovimentacaoId == movimentacao.Id)
                .ToListAsync();

            var vinculosReceber = await _context.ContasReceberMovimentacoes
                .Include(v => v.ContaReceber)
                .Where(v => v.MovimentacaoId == movimentacao.Id)
                .ToListAsync();

            if (vinculosPagar.Any(v => v.ContaPagar.Status == StatusDuplicataEnum.Paga) ||
                vinculosReceber.Any(v => v.ContaReceber.Status == StatusDuplicataEnum.Paga))
            {
                throw new InvalidOperationException("Não é permitido alterar movimentação vinculada a uma duplicata paga. Exclua a baixa antes de editar.");
            }

            foreach (var vinculo in vinculosPagar)
            {
                if (vinculo.ContaPagar.Status != StatusDuplicataEnum.Parcial)
                    throw new InvalidOperationException("Só é permitido alterar movimentações de duplicatas com status Parcial.");

                if (novoValor > vinculo.ContaPagar.ValorParcela)
                    throw new ArgumentException("O valor da movimentação não pode ser maior que o valor da duplicata.");

                vinculo.ValorBaixado = novoValor;
                var valorBaixado = await _context.ContasPagarMovimentacoes
                    .Where(v => v.ContaPagarId == vinculo.ContaPagarId && v.MovimentacaoId != vinculo.MovimentacaoId)
                    .SumAsync(v => v.ValorBaixado);
                AplicarStatus(vinculo.ContaPagar, valorBaixado + novoValor);
            }

            foreach (var vinculo in vinculosReceber)
            {
                if (vinculo.ContaReceber.Status != StatusDuplicataEnum.Parcial)
                    throw new InvalidOperationException("Só é permitido alterar movimentações de duplicatas com status Parcial.");

                if (novoValor > vinculo.ContaReceber.ValorParcela)
                    throw new ArgumentException("O valor da movimentação não pode ser maior que o valor da duplicata.");

                vinculo.ValorBaixado = novoValor;
                var valorBaixado = await _context.ContasReceberMovimentacoes
                    .Where(v => v.ContaReceberId == vinculo.ContaReceberId && v.MovimentacaoId != vinculo.MovimentacaoId)
                    .SumAsync(v => v.ValorBaixado);
                AplicarStatus(vinculo.ContaReceber, valorBaixado + novoValor);
            }
        }

        private async Task RemoverBaixasVinculadas(Movimentacao movimentacao)
        {
            var vinculosPagar = await _context.ContasPagarMovimentacoes
                .Include(v => v.ContaPagar)
                .Where(v => v.MovimentacaoId == movimentacao.Id)
                .ToListAsync();

            var vinculosReceber = await _context.ContasReceberMovimentacoes
                .Include(v => v.ContaReceber)
                .Where(v => v.MovimentacaoId == movimentacao.Id)
                .ToListAsync();

            foreach (var vinculo in vinculosPagar)
            {
                _context.ContasPagarMovimentacoes.Remove(vinculo);
                await RecalcularContaPagar(vinculo.ContaPagar, vinculo.MovimentacaoId);
            }

            foreach (var vinculo in vinculosReceber)
            {
                _context.ContasReceberMovimentacoes.Remove(vinculo);
                await RecalcularContaReceber(vinculo.ContaReceber, vinculo.MovimentacaoId);
            }

            if (vinculosPagar.Count > 0 || vinculosReceber.Count > 0)
                await _context.SaveChangesAsync();
        }

        private async Task RecalcularContaPagar(ContaPagar conta, int? movimentacaoIgnoradaId = null)
        {
            var valorBaixado = await _context.ContasPagarMovimentacoes
                .Where(v => v.ContaPagarId == conta.Id && (!movimentacaoIgnoradaId.HasValue || v.MovimentacaoId != movimentacaoIgnoradaId.Value))
                .SumAsync(v => v.ValorBaixado);

            conta.ValorBaixado = Math.Min(valorBaixado, conta.ValorParcela);
            conta.ValorRestante = Math.Max(conta.ValorParcela - conta.ValorBaixado, 0);
            conta.Status = conta.ValorBaixado <= 0
                ? StatusDuplicataEnum.Aberta
                : conta.ValorRestante > 0
                    ? StatusDuplicataEnum.Parcial
                    : StatusDuplicataEnum.Paga;
            conta.DataPagamento = conta.Status == StatusDuplicataEnum.Paga ? DateTime.UtcNow : null;
        }

        private async Task RecalcularContaReceber(ContaReceber conta, int? movimentacaoIgnoradaId = null)
        {
            var valorBaixado = await _context.ContasReceberMovimentacoes
                .Where(v => v.ContaReceberId == conta.Id && (!movimentacaoIgnoradaId.HasValue || v.MovimentacaoId != movimentacaoIgnoradaId.Value))
                .SumAsync(v => v.ValorBaixado);

            conta.ValorBaixado = Math.Min(valorBaixado, conta.ValorParcela);
            conta.ValorRestante = Math.Max(conta.ValorParcela - conta.ValorBaixado, 0);
            conta.Status = conta.ValorBaixado <= 0
                ? StatusDuplicataEnum.Aberta
                : conta.ValorRestante > 0
                    ? StatusDuplicataEnum.Parcial
                    : StatusDuplicataEnum.Paga;
            conta.DataPagamento = conta.Status == StatusDuplicataEnum.Paga ? DateTime.UtcNow : null;
        }

        private static void AplicarStatus(ContaPagar conta, decimal valorBaixado)
        {
            conta.ValorBaixado = Math.Min(valorBaixado, conta.ValorParcela);
            conta.ValorRestante = Math.Max(conta.ValorParcela - conta.ValorBaixado, 0);
            conta.Status = conta.ValorBaixado <= 0
                ? StatusDuplicataEnum.Aberta
                : conta.ValorRestante > 0
                    ? StatusDuplicataEnum.Parcial
                    : StatusDuplicataEnum.Paga;
            conta.DataPagamento = conta.Status == StatusDuplicataEnum.Paga ? DateTime.UtcNow : null;
        }

        private static void AplicarStatus(ContaReceber conta, decimal valorBaixado)
        {
            conta.ValorBaixado = Math.Min(valorBaixado, conta.ValorParcela);
            conta.ValorRestante = Math.Max(conta.ValorParcela - conta.ValorBaixado, 0);
            conta.Status = conta.ValorBaixado <= 0
                ? StatusDuplicataEnum.Aberta
                : conta.ValorRestante > 0
                    ? StatusDuplicataEnum.Parcial
                    : StatusDuplicataEnum.Paga;
            conta.DataPagamento = conta.Status == StatusDuplicataEnum.Paga ? DateTime.UtcNow : null;
        }

        private static void RecalcularRateioExistente(List<MovimentacaoCategoria> rateios, decimal novoTotal)
        {
            decimal acumulado = 0;

            for (var i = 0; i < rateios.Count; i++)
            {
                var valor = i == rateios.Count - 1
                    ? novoTotal - acumulado
                    : Math.Round(novoTotal * rateios[i].Percentual / 100, 2, MidpointRounding.AwayFromZero);

                acumulado += valor;
                rateios[i].Valor = valor;
            }
        }
    }
}
