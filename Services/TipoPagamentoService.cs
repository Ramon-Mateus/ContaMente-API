using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;

namespace ContaMente.Services
{
    public class TipoPagamentoService : ITipoPagamentoService
    {
        public List<TipoPagamentoDto> GetTiposPagamento()
        {
            return Enum.GetValues(typeof(TipoPagamentoEnum))
                .Cast<TipoPagamentoEnum>()
                .Select(e => new TipoPagamentoDto
                {
                    Id = (int)e,
                    Nome = e.GetDisplayName()
                })
                .ToList();
        }

        public TipoPagamentoDto? GetTipoPagamentoById(int id)
        {
            if (Enum.IsDefined(typeof(TipoPagamentoEnum), id))
            {
                return new TipoPagamentoDto
                {
                    Id = id,
                    Nome = ((TipoPagamentoEnum)id).GetDisplayName()
                };
            }

            return null;
        }
    }
}
