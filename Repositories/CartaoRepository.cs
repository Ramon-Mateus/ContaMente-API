using ContaMente.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Repositories
{
    public class CartaoRepository : ICartaoRepository
    {
        private readonly ApplicationDbContext _context;

        public CartaoRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<Cartao>> GetCartoes(string userId)
        {
            return await _context.Cartoes
                .Include(c => c.Movimentacoes)
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Apelido)
                .ToListAsync();
        }

        public async Task<Cartao?> GetCartaoById(int id, string userId)
        {
            return await _context.Cartoes
            .Include(c => c.Movimentacoes)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        }

        public async Task<Cartao> CreateCartao(Cartao cartao)
        {
            _context.Cartoes.Add(cartao);
            await _context.SaveChangesAsync();

            return cartao;
        }

        public async Task<Cartao?> UpdateCartao(Cartao cartao)
        {
            _context.Cartoes.Update(cartao);
            await _context.SaveChangesAsync();

            return cartao;
        }

        public async Task<bool> DeleteCartao(Cartao cartao)
        {
            _context.Cartoes.Remove(cartao);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExisteCartaoComApelido(string apelido, string userId, int? idCartaoUpdate)
        {
            if (idCartaoUpdate == null)
            {
                return await _context.Cartoes.
                    AnyAsync(c => c.Apelido.ToLower() == apelido.ToLower() && c.UserId == userId );
            }
            else
            {
                return await _context.Cartoes.
                    AnyAsync(c => c.Apelido.ToLower() == apelido.ToLower() && c.UserId == userId && c.Id != idCartaoUpdate);
            } 
        }
    }
}
