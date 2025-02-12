using ContaMente.Contexts;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Repositories
{
    public class RecorrenciaRepository : IRecorrenciaRepository
    {
        private readonly ApplicationDbContext _context;
        public RecorrenciaRepository(ApplicationDbContext dbContext) 
        {
            _context = dbContext;
        }

        public async Task<Recorrencia?> GetRecorrenciaById(int id)
        {
            return await _context.Recorrencias
                .Include(r => r.Movimentacoes)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Recorrencia> CreateRecorrencia(Recorrencia recorrencia)
        {
            _context.Recorrencias.Add(recorrencia);
            await _context.SaveChangesAsync();
            return recorrencia;
        }
    }
}
