using ContaMente.Models;

namespace ContaMente.Services.Interfaces
{
    public interface IParcelaService
    {
        Task<Parcela> CreateParcela(Parcela parcela);
    }
}
