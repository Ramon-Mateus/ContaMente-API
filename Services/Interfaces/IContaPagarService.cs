using ContaMente.DTOs;

namespace ContaMente.Services.Interfaces
{
    public interface IContaPagarService
    {
        Task<List<ContaPagarDto>> GetContasPagar(int[]? categoriasIds, int[]? responsaveisIds, DateTime? dataVencimentoInicio, DateTime? dataVencimentoFim, bool? pago, StatusDuplicataEnum? status, string userId);
        Task<ContaPagarDto?> GetContaPagarById(int id, string userId);
        Task<List<ContaPagarDto>> LancarContasPagar(CreateContaPagarDto dto, string userId);
        Task<ContaPagarDto?> UpdateContaPagar(int id, UpdateContaPagarDto dto, string userId);
        Task<ContaPagarDto?> BaixarContaPagar(int id, BaixaContaPagarDto dto, string userId);
        Task<ContaPagarDto?> MarcarComoPaga(int id, string userId);
        Task<bool> DeleteContaPagar(int id, string userId);
    }
}
