using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;

namespace ContaMente.Services
{
    public class ParcelaService : IParcelaService
    {
        private readonly IParcelaRepository _parcelaRepository;

        public ParcelaService(IParcelaRepository parcelaRepository) => _parcelaRepository = parcelaRepository;

        public Task<Parcela> CreateParcela(Parcela parcela)
        {
            return _parcelaRepository.CreateParcela(parcela);
        }
    }
}
