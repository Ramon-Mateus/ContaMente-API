using ContaMente.Models;

namespace ContaMente.Repositories.Interfaces;

public interface IGastoRepository
{
    Task<List<Gasto>> GetGastos();
    Task<Gasto?> GetGastoById(int id);
    Task<Gasto> CreateGasto(Gasto gasto);
    Task<Gasto?> UpdateGasto(Gasto gasto);
    Task<bool> DeleteGasto(Gasto gasto);
}