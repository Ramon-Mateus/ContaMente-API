using ContaMente.DTOs;
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
        #endregion
        
        #region Variaveis
        private IQueryable<Movimentacao>? _queryableMovimentacao;
        #endregion
        
        public MovimentacaoService(
            IMovimentacaoRepository movimentacaoRepository,
            IRecorrenciaService recorrenciaService,
            IMovimentacaoParcelaService movimentacaoParcelaService,
            IUserConfigurationService userConfigurationService,
            ICartaoRepository cartaoRepository)
        {
            _movimentacaoRepository = movimentacaoRepository;
            _recorrenciaService = recorrenciaService;
            _movimentacaoParcelaService = movimentacaoParcelaService;
            _userConfigurationService = userConfigurationService;
            _cartaoRepository = cartaoRepository;
        }

        public async Task<Dictionary<DateTime, List<MovimentacaoDto>>> GetMovimentacoes(
            int? mes,
            int? ano,
            string userId,
            bool entrada,
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
            bool entrada)
        {
            var userConfig = await _userConfigurationService.GetUserConfiguration(userId);

            if (userConfig!.ListagemPorFatura && mes.HasValue && ano.HasValue)
            {
                // Buscar todos os cartões do usuário
                var cartoes = await _cartaoRepository.GetCartoes(userId);

                // Movimentações sem cartão (cartaoId is null) do mês completo
                var queryMovsSemCartao = _queryableMovimentacao!.Where(m => m.CartaoId == null &&
                                                                                              m.Data.Month == mes.Value &&
                                                                                              m.Data.Year == ano.Value &&
                                                                                              m.Categoria!.Entrada == entrada);

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
                                                                                            m.Data < dataFimFatura
                                                                                            && m.Categoria!.Entrada == entrada);

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
                    _queryableMovimentacao = _queryableMovimentacao!.Where(m => m.Data.Month == mes.Value && m.Categoria!.Entrada == entrada);

                if (ano.HasValue)
                    _queryableMovimentacao = _queryableMovimentacao!.Where(m => m.Data.Year == ano.Value && m.Categoria!.Entrada == entrada);
            }
        }

        private void CategoriasETiposPagamentoFiltro(
            List<int> categoriasIds,
            List<int> tiposPagamentoIds)
        {
            if (categoriasIds.Count != 0)
                _queryableMovimentacao = _queryableMovimentacao!.Where(m => categoriasIds.Contains(m.CategoriaId));

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

        public async Task<Movimentacao> CreateMovimentacao(CreateMovimentacaoDto createMovimentacaoDto)
        {
            var movimentacao = new Movimentacao
            {
                Valor = createMovimentacaoDto.Valor,
                Descricao = createMovimentacaoDto.Descricao,
                Data = createMovimentacaoDto.Data.ToUniversalTime(),
                Fixa = createMovimentacaoDto.Fixa,
                CategoriaId = createMovimentacaoDto.CategoriaId,
                TipoPagamento = (TipoPagamentoEnum)createMovimentacaoDto.TipoPagamentoId,
                ParcelaId = createMovimentacaoDto.ParcelaId,
                NumeroParcela = createMovimentacaoDto.NumeroParcela,
                ResponsavelId = createMovimentacaoDto.ResponsavelId,
                CartaoId = createMovimentacaoDto.CartaoId
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
                    CartaoId = movimentacaoOriginal.CartaoId
                };

                await _movimentacaoRepository.CreateMovimentacao(novaMovimentacao);
            }
        }

        public async Task<Movimentacao?> UpdateMovimentacao(int id, UpdateMovimentacaoDto updateMovimentacaoDto, string userId)
        {
            var movimentacao = await _movimentacaoRepository.GetMovimentacaoById(id, userId);

            if (movimentacao == null)
            {
                return null;
            }

            if (updateMovimentacaoDto.Valor.HasValue)
            {
                movimentacao.Valor = updateMovimentacaoDto.Valor.Value;
            }

            if (updateMovimentacaoDto.Data.HasValue)
            {
                movimentacao.Data = updateMovimentacaoDto.Data.Value;
            }

            if (!string.IsNullOrEmpty(updateMovimentacaoDto.Descricao))
            {
                movimentacao.Descricao = updateMovimentacaoDto.Descricao;
            }

            if (updateMovimentacaoDto.CategoriaId.HasValue)
            {
                movimentacao.CategoriaId = updateMovimentacaoDto.CategoriaId.Value;
            }

            if (updateMovimentacaoDto.TipoPagamentoId.HasValue)
            {
                movimentacao.TipoPagamento = (TipoPagamentoEnum)updateMovimentacaoDto.TipoPagamentoId.Value;
            }

            movimentacao.ResponsavelId = updateMovimentacaoDto.ResponsavelId;
            movimentacao.CartaoId      = updateMovimentacaoDto.CartaoId;

            return await _movimentacaoRepository.UpdateMovimentacao(movimentacao);
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

            return await _movimentacaoRepository.DeleteMovimentacao(movimentacao);
        }
    }
}
