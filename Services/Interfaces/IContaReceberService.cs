using ContaMente.DTOs;

namespace ContaMente.Services.Interfaces
{
    public interface IContaReceberService
    {
        Task<List<ContaReceberDto>> GetContasReceber(int[]? categoriasIds, int[]? responsaveisIds, DateTime? dataVencimentoInicio, DateTime? dataVencimentoFim, bool? pago, StatusDuplicataEnum? status, string userId);
        Task<ContaReceberDto?> GetContaReceberById(int id, string userId);
        Task<List<ContaReceberDto>> LancarContasReceber(CreateContaReceberDto dto, string userId);
        Task<ContaReceberDto?> UpdateContaReceber(int id, UpdateContaReceberDto dto, string userId);
        Task<ContaReceberDto?> BaixarContaReceber(int id, BaixaContaReceberDto dto, string userId);
        Task<bool> DeleteContaReceber(int id, string userId);
    }
}
