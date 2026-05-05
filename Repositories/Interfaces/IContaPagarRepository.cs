using ContaMente.Models;

namespace ContaMente.Repositories.Interfaces
{
    public interface IContaPagarRepository
    {
        IQueryable<ContaPagar> GetContasPagar(string userId);
        Task<ContaPagar?> GetContaPagarById(int id, string userId);
        Task<ContaPagar> CreateContaPagar(ContaPagar contaPagar);
        Task<ContaPagar?> UpdateContaPagar(ContaPagar contaPagar);
        Task<bool> DeleteContaPagar(ContaPagar contaPagar);
    }
}
