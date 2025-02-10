using ContaMente.Models;

namespace ContaMente.Repositories.Interfaces
{
    public interface IParcelaRepository
    {
        Task<List<Parcela>> GetParcelas(string userId);
        Task<Parcela?> GetParcelaById(int id, string userId);
        Task<Parcela> CreateParcela(Parcela parcela);
        Task<bool> DeleteParcela(Parcela parcela);
    }
}
