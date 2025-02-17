using ContaMente.Models;

namespace ContaMente.Services.Interfaces
{
    public interface ITipoPagamentoService
    {
        Task<List<TipoPagamento>> GetTiposPagamento();
        Task<TipoPagamento?> GetTipoPagamentoById(int id);
    }
}
