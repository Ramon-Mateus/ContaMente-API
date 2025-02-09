using ContaMente.DTOs;
using ContaMente.Models;

namespace ContaMente.Services.Interfaces
{
    public interface IMovimentacaoService
    {
        Task<List<Movimentacao>> GetMovimentacoes(int? mes, int? ano, string userId);
        Task<Movimentacao?> GetMovimentacaoById(int id, string userId);
        Task<Movimentacao> CreateMovimentacao(CreateMovimentacaoDto createMovimentacaoDto);
        Task<Movimentacao?> UpdateMovimentacao(int id, UpdateMovimentacaoDto updateMovimentacaoDto, string userId);
        Task<bool> DeleteMovimentacao(int id, string userId);
    }
}
