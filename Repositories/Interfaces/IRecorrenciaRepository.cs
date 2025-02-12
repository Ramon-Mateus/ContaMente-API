using ContaMente.Models;

namespace ContaMente.Repositories.Interfaces
{
    public interface IRecorrenciaRepository
    {
        Task<Recorrencia?> GetRecorrenciaById(int id);
        Task<Recorrencia> CreateRecorrencia(Recorrencia recorrencia);
        Task UpdateRecorrencia(Recorrencia recorrencia);
    }
}
