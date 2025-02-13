using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;

namespace ContaMente.Services
{
    public class TipoPagamentoService : ITipoPagamentoService
    {
        private readonly ITipoPagamentoRepository _tipoPagamentoRepository;

        public TipoPagamentoService(ITipoPagamentoRepository tipoPagamentoRepository) => _tipoPagamentoRepository = tipoPagamentoRepository;

        public async Task<List<TipoPagamento>> GetTiposPagamento()
        {
            return await _tipoPagamentoRepository.GetTiposPagamento();
        }

        public async Task<TipoPagamento?> GetTipoPagamentoById(int id)
        {
            return await _tipoPagamentoRepository.GetTipoPagamentoById(id);
        }
    }
}
