using ContaMente.Contexts;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Repositories
{
    public class ContaReceberRepository : IContaReceberRepository
    {
        private readonly ApplicationDbContext _context;

        public ContaReceberRepository(ApplicationDbContext context) => _context = context;

        public IQueryable<ContaReceber> GetContasReceber(string userId)
        {
            return _context.ContasReceber
                .Include(c => c.Responsavel)
                .Include(c => c.Categoria)
                .Include(c => c.Movimentacoes)
                    .ThenInclude(cm => cm.Movimentacao)
                .Include(c => c.CategoriasRateio)
                    .ThenInclude(cc => cc.Categoria)
                .Where(c => c.UserId == userId);
        }

        public async Task<ContaReceber?> GetContaReceberById(int id, string userId)
        {
            return await _context.ContasReceber
                .Include(c => c.Responsavel)
                .Include(c => c.Categoria)
                .Include(c => c.Movimentacoes)
                    .ThenInclude(cm => cm.Movimentacao)
                .Include(c => c.CategoriasRateio)
                    .ThenInclude(cc => cc.Categoria)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        }

        public async Task<ContaReceber> CreateContaReceber(ContaReceber contaReceber)
        {
            _context.ContasReceber.Add(contaReceber);
            await _context.SaveChangesAsync();

            await _context.Entry(contaReceber).Reference(c => c.Responsavel).LoadAsync();
            await _context.Entry(contaReceber).Reference(c => c.Categoria).LoadAsync();
            await _context.Entry(contaReceber)
                .Collection(c => c.CategoriasRateio)
                .Query()
                .Include(c => c.Categoria)
                .LoadAsync();

            return contaReceber;
        }

        public async Task<ContaReceber?> UpdateContaReceber(ContaReceber contaReceber)
        {
            _context.ContasReceber.Update(contaReceber);
            await _context.SaveChangesAsync();

            await _context.Entry(contaReceber).Reference(c => c.Responsavel).LoadAsync();
            await _context.Entry(contaReceber).Reference(c => c.Categoria).LoadAsync();
            await _context.Entry(contaReceber)
                .Collection(c => c.CategoriasRateio)
                .Query()
                .Include(c => c.Categoria)
                .LoadAsync();

            return contaReceber;
        }

        public async Task<bool> DeleteContaReceber(ContaReceber contaReceber)
        {
            _context.ContasReceber.Remove(contaReceber);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
