using ContaMente.DTOs;
using ContaMente.Models;

namespace ContaMente.Services.Interfaces
{
    public interface IResponsavelService
    {
        Task<List<Responsavel>> GetResponsaveis(string useId);
        Task<Responsavel?> GetResponsavelById(int id, string userId);
        Task<Responsavel> CreateResponsavel(CreateUpdateResponsavelDto createResponsavelDto, string userId);
        Task<Responsavel?> UpdateResponsavel(int id, CreateUpdateResponsavelDto updateResponsavelDto, string userId);
        Task<bool> DeleteResponsavel(int id, string userId);
    }
}
