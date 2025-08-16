namespace ContaMente.Services.Interfaces
{
    public interface IMovimentacaoParcelaService
    {
        Task<bool> DeleteParcela(int id, string userId);
    }
}