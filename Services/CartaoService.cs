using ContaMente.Services.Interfaces;

namespace ContaMente.Services
{
    public class CartaoService : ICartaoService
    {
        private readonly ICartaoRepository _cartaoRepository;
        private readonly IMovimentacaoService _movimentacaoService;

        public CartaoService(ICartaoRepository cartaoRepository, IMovimentacaoService movimentacaoService)
        {
            _cartaoRepository = cartaoRepository;
            _movimentacaoService = movimentacaoService;
        }

        public async Task<List<Cartao>> GetCartoes(string userId)
        {
            return await _cartaoRepository.GetCartoes(userId);
        }

        public async Task<Cartao?> GetCartaoById(int id, string userId)
        {
            return await _cartaoRepository.GetCartaoById(id, userId);
        }

        public async Task<Cartao> CreateCartao(CreateUpdateCartaoDto createCartaoDto, string userId)
        {
            var cartaoExiste = await _cartaoRepository.ExisteCartaoComApelido(createCartaoDto.Apelido, userId);

            if (cartaoExiste)
            {
                throw new ArgumentException($"Já existe um cartão com o nome '{createCartaoDto.Apelido}'.");
            }

            var cartao = new Cartao
            {
                Apelido = createCartaoDto.Apelido,
                DiaFechamento = createCartaoDto.DiaFechamento,
                UserId = userId
            };

            return await _cartaoRepository.CreateCartao(cartao);
        }

        public async Task<Cartao?> UpdateCartao(int id, CreateUpdateCartaoDto updateCartaoDto, string userId)
        {
            var cartao = await this.GetCartaoById(id, userId);

            if (cartao == null)
            {
                return null;
            }

            var cartaoExiste = await _cartaoRepository.ExisteCartaoComApelido(updateCartaoDto.Apelido, userId);

            if (cartaoExiste)
            {
                throw new ArgumentException($"Já existe um cartão com o nome '{updateCartaoDto.Apelido}'.");
            }
            
            cartao.Apelido = updateCartaoDto.Apelido;
            cartao.DiaFechamento = updateCartaoDto.DiaFechamento;

            return await _cartaoRepository.UpdateCartao(cartao);
        }

        public async Task<bool> DeleteCartao(int id, string userId)
        {
            var cartao = await this.GetCartaoById(id, userId);

            if (cartao == null)
            {
                return false;
            }

            for (int i = 0; i < cartao.Movimentacoes.Count; i++)
            {
                await _movimentacaoService.DeleteMovimentacao(cartao.Movimentacoes[i].Id, userId);
            }

            return await _cartaoRepository.DeleteCartao(cartao);
        }
    }
}
