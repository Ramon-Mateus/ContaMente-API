using ContaMente.Models;

namespace ContaMente.Repositories.Interfaces
{
    public interface IContaReceberRepository
    {
        IQueryable<ContaReceber> GetContasReceber(string userId);
        Task<ContaReceber?> GetContaReceberById(int id, string userId);
        Task<ContaReceber> CreateContaReceber(ContaReceber contaReceber);
        Task<ContaReceber?> UpdateContaReceber(ContaReceber contaReceber);
        Task<bool> DeleteContaReceber(ContaReceber contaReceber);
    }
}
