using ContaMente.Contexts;
using ContaMente.Repositories.Interfaces;
using ContaMente.Models;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Repositories
{
    public class TipoPagamentoRepository : ITipoPagamentoRepository
    {
        private readonly ApplicationDbContext _context;

        public TipoPagamentoRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<TipoPagamento>> GetTiposPagamento()
        {
            return await _context.TiposPagamento
                .Include(tp => tp.Movimentacoes)
                .ToListAsync();
        }

        public async Task<TipoPagamento?> GetTipoPagamentoById(int id)
        {
            return await _context.TiposPagamento
                .Include(tp => tp.Movimentacoes)
                .FirstOrDefaultAsync(tp => tp.Id == id);
        }
    }
}
