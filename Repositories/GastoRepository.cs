using ContaMente.Contexts;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Repositories;

public class GastoRepository : IGastoRepository
{
    private readonly ApplicationDbContext _context;
    
    public GastoRepository(ApplicationDbContext context) => _context = context;
    
    public async Task<List<Gasto>> GetGastos()
    {
        return await _context.Gastos
            .Include(g => g.Categoria)
            .OrderByDescending(g => g.Data)
            .ToListAsync();
    }

    public async Task<Gasto?> GetGastoById(int id)
    {
        return await _context.Gastos.Include(g => g.Categoria).FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Gasto> CreateGasto(Gasto gasto)
    {
        _context.Gastos.Add(gasto);
        await _context.SaveChangesAsync();

        return gasto;
    }

    public async Task<Gasto?> UpdateGasto(Gasto gasto)
    {
        _context.Gastos.Update(gasto);
        await _context.SaveChangesAsync();

        return gasto;
    }

    public async Task<bool> DeleteGasto(Gasto gasto)
    {
        _context.Gastos.Remove(gasto);
        await _context.SaveChangesAsync();

        return true;
    }
}