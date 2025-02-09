using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Services
{
    public class MovimentacaoService : IMovimentacaoService
    {
        private readonly IMovimentacaoRepository _movimentacaoRepository;
        private readonly IParcelaService _parcelaService;
        public MovimentacaoService(IMovimentacaoRepository movimentacaoRepository, IParcelaService parcelaService)
        {
            _movimentacaoRepository = movimentacaoRepository;
            _parcelaService = parcelaService;
        }

        public async Task<List<Movimentacao>> GetMovimentacoes(int? mes, int? ano, string userId, bool entrada)
        {
            var query = _movimentacaoRepository.GetMovimentacoes(userId);

            if (mes.HasValue)
                query = query.Where(m => m.Data.Month == mes.Value);

            if (ano.HasValue)
                query = query.Where(m => m.Data.Year == ano.Value);

            query = query.Where(m => m.Categoria!.Entrada == entrada);

            return await query
                .OrderByDescending(g => g.Data)
                .ToListAsync();
        }

        public async Task<Movimentacao?> GetMovimentacaoById(int id, string userId)
        {
            return await _movimentacaoRepository.GetMovimentacaoById(id, userId);
        }

        public async Task<Movimentacao> CreateMovimentacao(CreateMovimentacaoDto createMovimentacaoDto)
        {
            var movimentacao = new Movimentacao
            {
                Valor = createMovimentacaoDto.Valor,
                Descricao = createMovimentacaoDto.Descricao,
                Data = createMovimentacaoDto.Data,
                Fixa = createMovimentacaoDto.Fixa,
                CategoriaId = createMovimentacaoDto.CategoriaId,
                TipoPagamentoId = createMovimentacaoDto.TipoPagamentoId,
                RecorrenciaId = createMovimentacaoDto.RecorrenciaId
            };

            if (createMovimentacaoDto.Parcela != null)
            {
                var parcela = new Parcela
                {
                    ValorTotal = createMovimentacaoDto.Parcela.ValorTotal,
                    ValorParcela = createMovimentacaoDto.Parcela.ValorParcela,
                    NumeroParcelas = createMovimentacaoDto.Parcela.NumeroParcelas,
                    DataInicio = createMovimentacaoDto.Parcela.DataInicio,
                    DataFim = createMovimentacaoDto.Parcela.DataFim
                };

                var parcelaCriada = await _parcelaService.CreateParcela(parcela);
                movimentacao.ParcelaId = parcelaCriada.Id;
                movimentacao.NumeroParcela = 1;
                movimentacao.Valor = parcela.ValorParcela;

                for (int i = 1; i < parcelaCriada.NumeroParcelas; i++)
                {
                    var dataParcela = parcelaCriada.DataInicio.AddMonths(i);
                    var movimentacaoParcela = new Movimentacao
                    {
                        Valor = parcelaCriada.ValorParcela,
                        Descricao = createMovimentacaoDto.Descricao,
                        Data = dataParcela,
                        Fixa = createMovimentacaoDto.Fixa,
                        CategoriaId = createMovimentacaoDto.CategoriaId,
                        TipoPagamentoId = createMovimentacaoDto.TipoPagamentoId,
                        RecorrenciaId = createMovimentacaoDto.RecorrenciaId,
                        ParcelaId = parcelaCriada.Id,
                        NumeroParcela = i + 1
                    };

                    await _movimentacaoRepository.CreateMovimentacao(movimentacaoParcela);
                }
            }

            return await _movimentacaoRepository.CreateMovimentacao(movimentacao);
        }

        public async Task<Movimentacao?> UpdateMovimentacao(int id, UpdateMovimentacaoDto updateMovimentacaoDto, string userId)
        {
            var movimentacao = await this.GetMovimentacaoById(id, userId);

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

            return await _movimentacaoRepository.UpdateMovimentacao(movimentacao);
        }

        public async Task<bool> DeleteMovimentacao(int id, string userId)
        {
            var movimentacao = await this.GetMovimentacaoById(id, userId);

            if (movimentacao == null)
            {
                return false;
            }

            return await _movimentacaoRepository.DeleteMovimentacao(movimentacao);
        }
    }
}
