using ContaMente.Models;

namespace ContaMente.Repositories.Interfaces
{
    public interface IResponsavelRepository
    {
        Task<List<Responsavel>> GetResponsaveis(string userId);
        Task<Responsavel?> GetResponsavelById(int id, string userId);
        Task<Responsavel> CreateResponsavel(Responsavel responsavel);
        Task<Responsavel?> UpdateResponsavel(Responsavel responsavel);
        Task<bool> DeleteResponsavel(Responsavel responsavel);
        Task<bool> ExisteResponsavelComNome(string nome, string userId, int? idResponsavelUpdate);
    }
}
