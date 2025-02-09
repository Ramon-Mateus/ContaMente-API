using ContaMente.Contexts;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;

namespace ContaMente.Repositories
{
    public class ParcelaRepository : IParcelaRepository
    {
        private readonly ApplicationDbContext _context;

        public ParcelaRepository(ApplicationDbContext context) => _context = context;

        public async Task<Parcela> CreateParcela(Parcela parcela)
        {
            _context.Parcelas.Add(parcela);
            await _context.SaveChangesAsync();
            return parcela;
        }
    }
}
