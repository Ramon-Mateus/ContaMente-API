using ContaMente.Models;

namespace ContaMente.Repositories.Interfaces;

public interface ICategoriaRepository
{
    Task<List<Categoria>> GetCategorias();
    Task<Categoria?> GetCategoriaById(int id);
    Task<Categoria> CreateCategoria(Categoria categoria);
    Task<Categoria?> UpdateCategoria(Categoria categoria);
    Task<bool> DeleteCategoria(Categoria categoria);
}