using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;

namespace ContaMente.Services;

public class CategoriaService : ICategoriaService
{
        private readonly ICategoriaRepository _categoriaRepository;

        public CategoriaService(ICategoriaRepository categoriaRepository) => _categoriaRepository = categoriaRepository;
        
        public async Task<List<Categoria>> GetCategorias(string userId, bool entrada)
        {
            return await _categoriaRepository.GetCategorias(userId, entrada);
        }
        
        public async Task<Categoria?> GetCategoriaById(int id, string userId)
        {
            return await _categoriaRepository.GetCategoriaById(id, userId);
        }
        
        public async Task<Categoria> CreateCategoria(CreateCategoriaDto createCategoriaDto, string userId)
        {
            var categoria = new Categoria
            {
                Nome = createCategoriaDto.Nome,
                Entrada = createCategoriaDto.Entrada,
                UserId = userId
            };

            return await _categoriaRepository.CreateCategoria(categoria);
        }
        
        public async Task<Categoria?> UpdateCategoria(int id, UpdateCategoriaDto updateCategoriaDto, string userId)
        {
            var categoria = await this.GetCategoriaById(id, userId);

            if (categoria == null)
            {
                return null;
            }
            
            categoria.Nome = updateCategoriaDto.Nome;

            return await _categoriaRepository.UpdateCategoria(categoria);
        }
        
        public async Task<bool> DeleteCategoria(int id, string userId)
        {
            var categoria = await this.GetCategoriaById(id, userId);

            if (categoria == null)
            {
                return false;
            }

            return await _categoriaRepository.DeleteCategoria(categoria);
        }
}