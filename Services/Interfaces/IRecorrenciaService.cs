using ContaMente.Models;

namespace ContaMente.Services.Interfaces
{
    public interface IRecorrenciaService
    {
        Task<Recorrencia?> GetRecorrenciaById(int id);
        Task<Recorrencia> CreateRecorrencia(Recorrencia recorrencia);
    }
}
