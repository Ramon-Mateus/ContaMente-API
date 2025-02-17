using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Repositories;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace ContaMente.Services
{
    public class ParcelaService : IParcelaService
    {
        private readonly IParcelaRepository _parcelaRepository;
        private readonly IMovimentacaoService _movimentacaoService;

        public ParcelaService(IParcelaRepository parcelaRepository, IMovimentacaoService movimentacaoService)
        {
            _parcelaRepository = parcelaRepository;
            _movimentacaoService = movimentacaoService;
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
                DataInicio = createParcelaDto.DataInicio
            };

            var parcelaCriada = await _parcelaRepository.CreateParcela(parcela);

            for (int i = 0; i < parcelaCriada.NumeroParcelas; i++)
            {
                var dataParcela = parcelaCriada.DataInicio.AddMonths(i);

                var movimentacaoParcela = new CreateMovimentacaoDto
                {
                    Valor = parcelaCriada.ValorParcela,
                    Descricao = createParcelaDto.Descricao,
                    Data = dataParcela,
                    Fixa = false,
                    CategoriaId = createParcelaDto.CategoriaId,
                    TipoPagamentoId = createParcelaDto.TipoPagamentoId,
                    ParcelaId = parcelaCriada.Id,
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

            await this.DeleteParcela(id, userId);

            return await this.CreateParcela(createParcelaDto);
        }

        public async Task<bool> DeleteParcela(int id, string userId)
        {
            var parcela = await this.GetParcelaById(id, userId);

            if (parcela == null)
            {
                return false;
            }

            return await _parcelaRepository.DeleteParcela(parcela);
        }
    }
}
