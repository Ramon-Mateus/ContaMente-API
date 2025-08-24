using ContaMente.DTOs;
using ContaMente.Models;

namespace ContaMente.Services.Interfaces
{
    public interface IMovimentacaoService
    {
        Task<Dictionary<DateTime, List<MovimentacaoDto>>> GetMovimentacoes(int? mes, int? ano, string userId, bool entrada, List<int> categoriasIds, List<int> tiposPagamentoIds, List<int> responsaveisIds, List<int> cartoesIds);
        Task<MovimentacaoDto?> GetMovimentacaoById(int id, string userId);
        Task<Movimentacao> CreateMovimentacao(CreateMovimentacaoDto createMovimentacaoDto);
        Task<Movimentacao?> UpdateMovimentacao(int id, UpdateMovimentacaoDto updateMovimentacaoDto, string userId);
        Task<bool> DeleteMovimentacao(int id, string userId);
    }
}
