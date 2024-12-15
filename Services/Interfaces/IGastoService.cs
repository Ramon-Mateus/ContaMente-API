using ContaMente.DTOs;
using ContaMente.Models;

namespace ContaMente.Services.Interfaces
{
    public interface IGastoService
    {
        Task<List<Gasto>> GetGastos(int? mes, int? ano);
        Task<Gasto?> GetGastoById(int id);
        Task<Gasto> CreateGasto(CreateGastoDto createGastoDto);
        Task<Gasto?> UpdateGasto(int id, UpdateGastoDto updateGastoDto);
        Task<bool> DeleteGasto(int id);
    }
}
