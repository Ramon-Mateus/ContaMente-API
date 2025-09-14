public interface ICartaoRepository
{
        Task<List<Cartao>> GetCartoes(string userId);
        Task<Cartao?> GetCartaoById(int id, string userId);
        Task<Cartao> CreateCartao(Cartao cartao);
        Task<Cartao?> UpdateCartao(Cartao cartao);
        Task<bool> DeleteCartao(Cartao cartao);
        Task<bool> ExisteCartaoComApelido(string apelido, string userId);
}
