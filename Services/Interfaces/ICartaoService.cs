public interface ICartaoService
{
        Task<List<Cartao>> GetCartoes(string userId);
        Task<Cartao?> GetCartaoById(int id, string userId);
        Task<Cartao> CreateCartao(CreateUpdateCartaoDto createCartaoDto, string userId);
        Task<Cartao?> UpdateCartao(int id, CreateUpdateCartaoDto updateCartaoDto, string userId);
        Task<bool> DeleteCartao(int id, string userId);
}
