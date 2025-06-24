using ContaMente.DTOs;
using ContaMente.Models;

namespace ContaMente.Services.Interfaces
{
    public interface IMovimentacaoService
    {
        Task<Dictionary<DateTime, List<Movimentacao>>> GetMovimentacoes(int? mes, int? ano, string userId, bool entrada, List<int> categoriasIds, List<int> tiposPagamentoIds, List<int> responsaveisIds);
        Task<Movimentacao?> GetMovimentacaoById(int id, string userId);
        Task<Movimentacao> CreateMovimentacao(CreateMovimentacaoDto createMovimentacaoDto);
        Task<Movimentacao?> UpdateMovimentacao(int id, UpdateMovimentacaoDto updateMovimentacaoDto, string userId);
        Task<bool> DeleteMovimentacao(int id, string userId);
    }
}
