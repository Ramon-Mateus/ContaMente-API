using ContaMente.Models;

namespace ContaMente.Repositories.Interfaces
{
    public interface ITipoPagamentoRepository
    {
        Task<List<TipoPagamento>> GetTiposPagamento();
        Task<TipoPagamento?> GetTipoPagamentoById(int id);
    }
}
