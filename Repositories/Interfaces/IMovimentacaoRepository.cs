using ContaMente.Models;

namespace ContaMente.Repositories.Interfaces
{
    public interface IMovimentacaoRepository
    {
        IQueryable<Movimentacao> GetMovimentacoes(string userId);
        Task<Movimentacao?> GetMovimentacaoById(int id, string userId);
        Task<Movimentacao> CreateMovimentacao(Movimentacao movimentacao);
        Task<Movimentacao?> UpdateMovimentacao(Movimentacao movimentacao);
        Task<bool> DeleteMovimentacao(Movimentacao movimentacao);
    }
}
