using ContaMente.Contexts;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Repositories
{
    public class ResponsavelRepository : IResponsavelRepository
    {
        private readonly ApplicationDbContext _context;

        public ResponsavelRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<Responsavel>> GetResponsaveis(string userId)
        {
            return await _context.Responsaveis
                .Include(r => r.Movimentacoes)
                .Where(r => r.UserId == userId)
                .OrderBy(r => r.Nome)
                .ToListAsync();
        }

        public async Task<Responsavel?> GetResponsavelById(int id, string userId)
        {
            return await _context.Responsaveis
            .Include(c => c.Movimentacoes)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        }

        public async Task<Responsavel> CreateResponsavel(Responsavel responsavel)
        {
            _context.Responsaveis.Add(responsavel);
            await _context.SaveChangesAsync();

            return responsavel;
        }

        public async Task<Responsavel?> UpdateResponsavel(Responsavel responsavel)
        {
            _context.Responsaveis.Update(responsavel);
            await _context.SaveChangesAsync();

            return responsavel;
        }

        public async Task<bool> DeleteResponsavel(Responsavel responsavel)
        {
            _context.Responsaveis.Remove(responsavel);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
