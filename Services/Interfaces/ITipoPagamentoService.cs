using ContaMente.Models;

namespace ContaMente.Services.Interfaces
{
    public interface ITipoPagamentoService
    {
        List<TipoPagamentoDto> GetTiposPagamento();
        TipoPagamentoDto? GetTipoPagamentoById(int id);
    }
}
