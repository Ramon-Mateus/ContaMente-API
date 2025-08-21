using ContaMente.Contexts;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace ContaMente.Repositories
{
    public class MovimentacaoRepository : IMovimentacaoRepository
    {
        private readonly ApplicationDbContext _context;

        public MovimentacaoRepository(ApplicationDbContext context) => _context = context;

        public IQueryable<Movimentacao> GetMovimentacoes(string userId)
        {
            return _context.Movimentacoes
                .Include(m => m.Categoria)
                .Include(m => m.Recorrencia)
                .Include(m => m.Parcela)
                .Include(m => m.Responsavel)
                .Where(m => m.Categoria!.UserId == userId);
        }

        public async Task<Movimentacao?> GetMovimentacaoById(int id, string userId)
        {
            return await _context.Movimentacoes
                .Include(m => m.Categoria)
                .Include(m => m.Recorrencia)
                .Include(m => m.Parcela)
                .Include(m => m.Responsavel)
                .FirstOrDefaultAsync(g => g.Id == id && g.Categoria!.UserId == userId);
        }

        public async Task<Movimentacao> CreateMovimentacao(Movimentacao movimentacao)
        {
            _context.Movimentacoes.Add(movimentacao);
            await _context.SaveChangesAsync();

            return movimentacao;
        }

        public async Task<Movimentacao?> UpdateMovimentacao(Movimentacao movimentacao)
        {
            _context.Movimentacoes.Update(movimentacao);
            await _context.SaveChangesAsync();

            return movimentacao;
        }

        public async Task<bool> DeleteMovimentacao(Movimentacao movimentacao)
        {
            _context.Movimentacoes.Remove(movimentacao);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
