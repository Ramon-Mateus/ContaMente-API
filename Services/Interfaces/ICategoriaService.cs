using ContaMente.DTOs;
using ContaMente.Models;

namespace ContaMente.Services.Interfaces
{
    public interface ICategoriaService
    {
        Task<List<Categoria>> GetCategorias();
        Task<Categoria?> GetCategoriaById(int id);
        Task<Categoria> CreateCategoria(CreateCategoriaDto createCategoriaDto, string userId);
        Task<Categoria?> UpdateCategoria(int id, UpdateCategoriaDto updateCategoriaDto);
        Task<bool> DeleteCategoria(int id);
    }
}
