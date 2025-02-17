using ContaMente.Contexts;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.Arm;

namespace ContaMente.Repositories
{
    public class ParcelaRepository : IParcelaRepository
    {
        private readonly ApplicationDbContext _context;

        public ParcelaRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<Parcela>> GetParcelas(string userId)
        {
            return await _context.Parcelas
                .Include(p => p.Movimentacoes)
                .Where(p => p.Movimentacoes.Any(m => m.Categoria!.UserId == userId))
                .ToListAsync();
        }

        public async Task<Parcela?> GetParcelaById(int id, string userId)
        {
            return await _context.Parcelas
                .Include(p => p.Movimentacoes)
                .FirstOrDefaultAsync(p => p.Id == id && p.Movimentacoes.Any(m => m.Categoria!.UserId == userId));
        }

        public async Task<Parcela> CreateParcela(Parcela parcela)
        {
            _context.Parcelas.Add(parcela);
            await _context.SaveChangesAsync();
            return parcela;
        }

        public async Task<bool> DeleteParcela(Parcela parcela)
        {
            var movimentacoes = await _context.Movimentacoes
                .Where(m => m.ParcelaId == parcela.Id)
                .ToListAsync();

            foreach (var movimentacao in movimentacoes)
            {
                _context.Movimentacoes.Remove(movimentacao);
            }

            _context.Parcelas.Remove(parcela);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
