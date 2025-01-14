using ContaMente.DTOs;
using ContaMente.Models;

namespace ContaMente.Services.Interfaces
{
    public interface IGastoService
    {
        Task<List<Gasto>> GetGastos(int? mes, int? ano, string userId);
        Task<Gasto?> GetGastoById(int id, string userId);
        Task<Gasto> CreateGasto(CreateGastoDto createGastoDto);
        Task<Gasto?> UpdateGasto(int id, UpdateGastoDto updateGastoDto, string userId);
        Task<bool> DeleteGasto(int id, string userId);
    }
}
