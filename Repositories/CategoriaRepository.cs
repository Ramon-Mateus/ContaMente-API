using ContaMente.Contexts;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
            private readonly ApplicationDbContext _context;
    
            public CategoriaRepository(ApplicationDbContext context) => _context = context;
            
            public async Task<List<Categoria>> GetCategorias(string userId, bool entrada)
            {
                return await _context.Categorias
                    .Include(c => c.Movimentacoes)
                    .Where(c => c.UserId == userId && c.Entrada == entrada)
                    .OrderBy(c => c.Nome)
                    .ToListAsync();
            }
            
            public async Task<Categoria?> GetCategoriaById(int id, string userId)
            {
                return await _context.Categorias
                .Include(c => c.Movimentacoes)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            }
            
            public async Task<Categoria> CreateCategoria(Categoria categoria)
            {
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();
    
                return categoria;
            }
            
            public async Task<Categoria?> UpdateCategoria(Categoria categoria)
            {
                _context.Categorias.Update(categoria);
                await _context.SaveChangesAsync();
    
                return categoria;
            }
            
            public async Task<bool> DeleteCategoria(Categoria categoria)
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
                
                return true;
            }
}