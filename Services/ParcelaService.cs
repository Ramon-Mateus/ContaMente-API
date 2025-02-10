using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;

namespace ContaMente.Services
{
    public class ParcelaService : IParcelaService
    {
        private readonly IParcelaRepository _parcelaRepository;

        public ParcelaService(IParcelaRepository parcelaRepository) => _parcelaRepository = parcelaRepository;

        public Task<List<Parcela>> GetParcelas(string userId)
        {
            return _parcelaRepository.GetParcelas(userId);
        }

        public Task<Parcela?> GetParcelaById(int id, string userId)
        {
            return _parcelaRepository.GetParcelaById(id, userId);
        }

        public Task<Parcela> CreateParcela(Parcela parcela)
        {
            return _parcelaRepository.CreateParcela(parcela);
        }

        public async Task<bool> DeleteParcela(int id, string userId)
        {
            var parcela = await this.GetParcelaById(id, userId);

            if (parcela == null)
            {
                return false;
            }

            return await _parcelaRepository.DeleteParcela(parcela);
        }
    }
}
