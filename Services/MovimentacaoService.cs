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
        public MovimentacaoService(IMovimentacaoRepository movimentacaoRepository)
        {
            _movimentacaoRepository = movimentacaoRepository;
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
                RecorrenciaId = createMovimentacaoDto.RecorrenciaId,
                ParcelaId = createMovimentacaoDto.ParcelaId
            };

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

            if (updateMovimentacaoDto.TipoPagamentoId.HasValue)
            {
                movimentacao.CategoriaId = updateMovimentacaoDto.TipoPagamentoId.Value;
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
