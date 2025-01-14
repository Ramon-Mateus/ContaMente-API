using ContaMente.DTOs;
using ContaMente.Models;

namespace ContaMente.Services.Interfaces
{
    public interface ICategoriaService
    {
        Task<List<Categoria>> GetCategorias(string userId);
        Task<Categoria?> GetCategoriaById(int id, string userId);
        Task<Categoria> CreateCategoria(CreateCategoriaDto createCategoriaDto, string userId);
        Task<Categoria?> UpdateCategoria(int id, UpdateCategoriaDto updateCategoriaDto, string userId);
        Task<bool> DeleteCategoria(int id, string userId);
    }
}
