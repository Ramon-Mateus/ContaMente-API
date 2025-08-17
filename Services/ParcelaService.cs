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

        public ParcelaService(IParcelaRepository parcelaRepository, IMovimentacaoService movimentacaoService, IMovimentacaoParcelaService movimentacaoParcelaService)
        {
            _parcelaRepository = parcelaRepository;
            _movimentacaoService = movimentacaoService;
            _movimentacaoParcelaService = movimentacaoParcelaService;
        }

        public Task<List<Parcela>> GetParcelas(string userId)
        {
            return _parcelaRepository.GetParcelas(userId);
        }

        public Task<Parcela?> GetParcelaById(int id, string userId)
        {
            return _parcelaRepository.GetParcelaById(id, userId);
        }

        public async Task<Parcela> CreateParcela(CreateParcelaDto createParcelaDto)
        {
            var parcela = new Parcela
            {
                ValorTotal = createParcelaDto.ValorTotal,
                ValorParcela = createParcelaDto.ValorParcela,
                NumeroParcelas = createParcelaDto.NumeroParcelas,
                DataInicio = createParcelaDto.DataInicio.ToUniversalTime()
            };

            var parcelaCriada = await _parcelaRepository.CreateParcela(parcela);

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
                    NumeroParcela = i + 1
                };

                await _movimentacaoService.CreateMovimentacao(movimentacaoParcela);
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

            return await this.CreateParcela(createParcelaDto);
        }
    }
}
