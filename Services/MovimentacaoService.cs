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
        private readonly IMovimentacaoRepository _movimentacaoRepository;
        private readonly IRecorrenciaService _recorrenciaService;
        private readonly IMovimentacaoParcelaService _movimentacaoParcelaService;
        public MovimentacaoService(IMovimentacaoRepository movimentacaoRepository, IRecorrenciaService recorrenciaService, IMovimentacaoParcelaService movimentacaoParcelaService)
        {
            _movimentacaoRepository = movimentacaoRepository;
            _recorrenciaService = recorrenciaService;
            _movimentacaoParcelaService = movimentacaoParcelaService;
        }

        public async Task<Dictionary<DateTime, List<Movimentacao>>> GetMovimentacoes(
            int? mes,
            int? ano,
            string userId,
            bool entrada,
            List<int> categoriasIds,
            List<int> tiposPagamentoIds,
            List<int> responsaveisIds)
        {
            var query = _movimentacaoRepository.GetMovimentacoes(userId);

            if (mes.HasValue)
                query = query.Where(m => m.Data.Month == mes.Value);

            if (ano.HasValue)
                query = query.Where(m => m.Data.Year == ano.Value);

            query = query.Where(m => m.Categoria!.Entrada == entrada);

            if (categoriasIds.Count != 0)
                query = query.Where(m => categoriasIds.Contains(m.CategoriaId));

            if (tiposPagamentoIds.Count != 0)
                query = query.Where(m => tiposPagamentoIds.Contains(m.TipoPagamentoId));

            if (responsaveisIds.Count != 0)
                query = query.Where(m => m.ResponsavelId.HasValue && responsaveisIds.Contains(m.ResponsavelId.Value));

            var movimentacoes = await query
                .OrderByDescending(m => m.Data)
                .ToListAsync();

            var movimentacoesPorDia = movimentacoes
                .GroupBy(m => m.Data.Date.AddDays(1))
                .ToDictionary(g => g.Key, g => g.ToList());

            return movimentacoesPorDia;
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
                ParcelaId = createMovimentacaoDto.ParcelaId,
                ResponsavelId = createMovimentacaoDto.ResponsavelId
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
                    TipoPagamentoId = movimentacaoOriginal.TipoPagamentoId,
                    RecorrenciaId = recorrencia.Id,
                    ResponsavelId = movimentacaoOriginal.ResponsavelId
                };

                await _movimentacaoRepository.CreateMovimentacao(novaMovimentacao);
            }
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
                movimentacao.TipoPagamentoId = updateMovimentacaoDto.TipoPagamentoId.Value;
            }

            movimentacao.ResponsavelId = updateMovimentacaoDto.ResponsavelId;

            return await _movimentacaoRepository.UpdateMovimentacao(movimentacao);
        }

        public async Task<bool> DeleteMovimentacao(int id, string userId)
        {
            var movimentacao = await this.GetMovimentacaoById(id, userId);

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
