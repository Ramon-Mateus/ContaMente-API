using ContaMente.Contexts;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Repositories
{
    public class ContaPagarRepository : IContaPagarRepository
    {
        private readonly ApplicationDbContext _context;

        public ContaPagarRepository(ApplicationDbContext context) => _context = context;

        public IQueryable<ContaPagar> GetContasPagar(string userId)
        {
            return _context.ContasPagar
                .Include(c => c.Responsavel)
                .Include(c => c.Categoria)
                .Include(c => c.Movimentacoes)
                    .ThenInclude(cm => cm.Movimentacao)
                .Include(c => c.CategoriasRateio)
                    .ThenInclude(cc => cc.Categoria)
                .Where(c => c.UserId == userId);
        }

        public async Task<ContaPagar?> GetContaPagarById(int id, string userId)
        {
            return await _context.ContasPagar
                .Include(c => c.Responsavel)
                .Include(c => c.Categoria)
                .Include(c => c.Movimentacoes)
                    .ThenInclude(cm => cm.Movimentacao)
                .Include(c => c.CategoriasRateio)
                    .ThenInclude(cc => cc.Categoria)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        }

        public async Task<ContaPagar> CreateContaPagar(ContaPagar contaPagar)
        {
            _context.ContasPagar.Add(contaPagar);
            await _context.SaveChangesAsync();
            
            await _context.Entry(contaPagar).Reference(c => c.Responsavel).LoadAsync();
            await _context.Entry(contaPagar).Reference(c => c.Categoria).LoadAsync();
            await _context.Entry(contaPagar)
                .Collection(c => c.CategoriasRateio)
                .Query()
                .Include(c => c.Categoria)
                .LoadAsync();

            return contaPagar;
        }

        public async Task<ContaPagar?> UpdateContaPagar(ContaPagar contaPagar)
        {
            _context.ContasPagar.Update(contaPagar);
            await _context.SaveChangesAsync();

            await _context.Entry(contaPagar).Reference(c => c.Responsavel).LoadAsync();
            await _context.Entry(contaPagar).Reference(c => c.Categoria).LoadAsync();
            await _context.Entry(contaPagar)
                .Collection(c => c.CategoriasRateio)
                .Query()
                .Include(c => c.Categoria)
                .LoadAsync();

            return contaPagar;
        }

        public async Task<bool> DeleteContaPagar(ContaPagar contaPagar)
        {
            _context.ContasPagar.Remove(contaPagar);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
