using ContaMente.Models;

namespace ContaMente.Repositories.Interfaces
{
    public interface IParcelaRepository
    {
        Task<Parcela> CreateParcela(Parcela parcela);
    }
}
