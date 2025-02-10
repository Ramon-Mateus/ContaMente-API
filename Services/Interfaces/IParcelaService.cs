using ContaMente.Models;

namespace ContaMente.Services.Interfaces
{
    public interface IParcelaService
    {
        Task<List<Parcela>> GetParcelas(string userId);
        Task<Parcela?> GetParcelaById(int id, string userId);
        Task<Parcela> CreateParcela(Parcela parcela);
        Task<bool> DeleteParcela(int id, string userId);
    }
}
