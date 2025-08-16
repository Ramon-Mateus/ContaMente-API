using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;

namespace ContaMente.Services
{
    public class MovimentacaoParcelaService : IMovimentacaoParcelaService
    {
        private readonly IParcelaRepository _parcelaRepository;

        public MovimentacaoParcelaService(IParcelaRepository parcelaRepository)
        {
            _parcelaRepository = parcelaRepository;
        }

        public async Task<bool> DeleteParcela(int id, string userId)
        {
            var parcela = await _parcelaRepository.GetParcelaById(id, userId);  

            if (parcela == null)
            {
                return false;
            }

            return await _parcelaRepository.DeleteParcela(parcela);
        }
    }
}