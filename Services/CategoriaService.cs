using ContaMente.Contexts;
using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Services;

public class CategoriaService : ICategoriaService
{
        private readonly ApplicationDbContext _context;

        public CategoriaService(ApplicationDbContext context) => _context = context;
        
        public async Task<List<Categoria>> GetCategorias()
        {
            var categorias = await _context.Categorias.Include(c => c.Gastos).ToListAsync();

            return categorias;
        }
        
        public async Task<Categoria?> GetCategoriaById(int id)
        {
            var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id);
            
            return categoria;
        }
        
        public async Task<Categoria> CreateCategoria(CreateCategoriaDto createCategoriaDto)
        {
            var categoria = new Categoria
            {
                Nome = createCategoriaDto.Nome
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return categoria;
        }
        
        public async Task<Categoria?> UpdateCategoria(int id, UpdateCategoriaDto updateCategoriaDto)
        {
            var categoria = await this.GetCategoriaById(id);

            if (categoria == null)
            {
                return null;
            }
            
            categoria.Nome = updateCategoriaDto.Nome;

            await _context.SaveChangesAsync();

            return categoria;
        }
        
        public async Task<bool> DeleteCategoria(int id)
        {
            var categoria = await this.GetCategoriaById(id);

            if (categoria == null)
            {
                return false;
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
            
            return true;
        }
}