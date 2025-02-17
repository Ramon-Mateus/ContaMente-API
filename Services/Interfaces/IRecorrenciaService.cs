using ContaMente.Models;

namespace ContaMente.Services.Interfaces
{
    public interface IRecorrenciaService
    {
        Task<Recorrencia?> GetRecorrenciaById(int id);
        Task<Recorrencia> CreateRecorrencia(Recorrencia recorrencia);
        Task CancelarRecorrencia(int recorrenciaId, string userId);
    }
}
