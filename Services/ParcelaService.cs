using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;

namespace ContaMente.Services
{
    public class ParcelaService : IParcelaService
    {
        private readonly IParcelaRepository _parcelaRepository;
        private readonly IMovimentacaoService _movimentacaoService;
        private readonly IMovimentacaoParcelaService _movimentacaoParcelaService;
        private readonly ICartaoService _cartaoService;

        public ParcelaService(
            IParcelaRepository parcelaRepository,
            IMovimentacaoService movimentacaoService,
            IMovimentacaoParcelaService movimentacaoParcelaService,
            ICartaoService cartaoService)
        {
            _parcelaRepository = parcelaRepository;
            _movimentacaoService = movimentacaoService;
            _movimentacaoParcelaService = movimentacaoParcelaService;
            _cartaoService = cartaoService;
        }

        public Task<List<Parcela>> GetParcelas(string userId)
        {
            return _parcelaRepository.GetParcelas(userId);
        }

        public Task<Parcela?> GetParcelaById(int id, string userId)
        {
            return _parcelaRepository.GetParcelaById(id, userId);
        }

        public async Task<Parcela> CreateParcela(CreateParcelaDto createParcelaDto, string userId)
        {
            var parcela = new Parcela
            {
                ValorTotal = createParcelaDto.ValorTotal,
                ValorParcela = createParcelaDto.ValorParcela,
                NumeroParcelas = createParcelaDto.NumeroParcelas,
                DataInicio = createParcelaDto.DataInicio.ToUniversalTime()
            };

            var parcelaCriada = await _parcelaRepository.CreateParcela(parcela);

            if (createParcelaDto.CartaoId.HasValue)
            {
                var cartao = await _cartaoService.GetCartaoById(createParcelaDto.CartaoId.Value, userId);

                if (cartao == null)
                {
                    throw new KeyNotFoundException($"Cartão com ID {createParcelaDto.CartaoId.Value} não encontrado.");
                }

                var movimentacaoParcela = new CreateMovimentacaoDto
                {
                    Valor = parcelaCriada.ValorParcela,
                    Descricao = createParcelaDto.Descricao,
                    Data = createParcelaDto.DataInicio.ToUniversalTime(),
                    Fixa = false,
                    CategoriaId = createParcelaDto.CategoriaId,
                    TipoPagamentoId = createParcelaDto.TipoPagamentoId,
                    ParcelaId = parcelaCriada.Id,
                    ResponsavelId = createParcelaDto.ResponsavelId,
                    NumeroParcela = 1,
                    CartaoId = createParcelaDto.CartaoId
                };

                await _movimentacaoService.CreateMovimentacao(movimentacaoParcela);

                DateTime dataProximaMov = createParcelaDto.DataInicio.Day >= cartao.DiaFechamento
                    ? new DateTime(createParcelaDto.DataInicio.Year, createParcelaDto.DataInicio.Month, cartao.DiaFechamento).AddMonths(1)
                    : new DateTime(createParcelaDto.DataInicio.Year, createParcelaDto.DataInicio.Month, cartao.DiaFechamento);

                for (int i = 1; i < parcelaCriada.NumeroParcelas; i++)
                {
                    movimentacaoParcela = new CreateMovimentacaoDto
                    {
                        Valor = parcelaCriada.ValorParcela,
                        Descricao = createParcelaDto.Descricao,
                        Data = dataProximaMov.ToUniversalTime(),
                        Fixa = false,
                        CategoriaId = createParcelaDto.CategoriaId,
                        TipoPagamentoId = createParcelaDto.TipoPagamentoId,
                        ParcelaId = parcelaCriada.Id,
                        ResponsavelId = createParcelaDto.ResponsavelId,
                        NumeroParcela = i + 1,
                        CartaoId = createParcelaDto.CartaoId
                    };

                    await _movimentacaoService.CreateMovimentacao(movimentacaoParcela);

                    dataProximaMov = dataProximaMov.AddMonths(1);
                }
            }
            else
            {
                for (int i = 0; i < parcelaCriada.NumeroParcelas; i++)
                {
                    var dataParcela = parcelaCriada.DataInicio.AddMonths(i);

                    var movimentacaoParcela = new CreateMovimentacaoDto
                    {
                        Valor = parcelaCriada.ValorParcela,
                        Descricao = createParcelaDto.Descricao,
                        Data = dataParcela.ToUniversalTime(),
                        Fixa = false,
                        CategoriaId = createParcelaDto.CategoriaId,
                        TipoPagamentoId = createParcelaDto.TipoPagamentoId,
                        ParcelaId = parcelaCriada.Id,
                        ResponsavelId = createParcelaDto.ResponsavelId,
                        NumeroParcela = i + 1,
                        CartaoId = createParcelaDto.CartaoId
                    };

                    await _movimentacaoService.CreateMovimentacao(movimentacaoParcela);
                }
            }

            return parcelaCriada;
        }

        public async Task<Parcela?> UpdateParcela(int id, CreateParcelaDto createParcelaDto, string userId)
        {
            var parcela = await this.GetParcelaById(id, userId);

            if (parcela == null)
            {
                return null;
            }

            await _movimentacaoParcelaService.DeleteParcela(id, userId);

            return await this.CreateParcela(createParcelaDto, userId);
        }
    }
}
