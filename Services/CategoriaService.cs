using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;

namespace ContaMente.Services;

public class CategoriaService : ICategoriaService
{
        private readonly ICategoriaRepository _categoriaRepository;

        public CategoriaService(ICategoriaRepository categoriaRepository) => _categoriaRepository = categoriaRepository;
        
        public async Task<List<Categoria>> GetCategorias()
        {
            return await _categoriaRepository.GetCategorias();
        }
        
        public async Task<Categoria?> GetCategoriaById(int id)
        {
            return await _categoriaRepository.GetCategoriaById(id);
        }
        
        public async Task<Categoria> CreateCategoria(CreateCategoriaDto createCategoriaDto)
        {
            var categoria = new Categoria
            {
                Nome = createCategoriaDto.Nome
            };

            return await _categoriaRepository.CreateCategoria(categoria);
        }
        
        public async Task<Categoria?> UpdateCategoria(int id, UpdateCategoriaDto updateCategoriaDto)
        {
            var categoria = await this.GetCategoriaById(id);

            if (categoria == null)
            {
                return null;
            }
            
            categoria.Nome = updateCategoriaDto.Nome;

            return await _categoriaRepository.UpdateCategoria(categoria);
        }
        
        public async Task<bool> DeleteCategoria(int id)
        {
            var categoria = await this.GetCategoriaById(id);

            if (categoria == null)
            {
                return false;
            }

            return await _categoriaRepository.DeleteCategoria(categoria);
        }
}