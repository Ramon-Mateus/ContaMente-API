using ContaMente.Models;

namespace ContaMente.Repositories.Interfaces;

public interface ICategoriaRepository
{
    Task<List<Categoria>> GetCategorias(string userId, bool entrada);
    Task<Categoria?> GetCategoriaById(int id, string userId);
    Task<Categoria> CreateCategoria(Categoria categoria);
    Task<Categoria?> UpdateCategoria(Categoria categoria);
    Task<bool> DeleteCategoria(Categoria categoria);
    Task<bool> ExisteCategoriaComNome(string nome, string userId);
}