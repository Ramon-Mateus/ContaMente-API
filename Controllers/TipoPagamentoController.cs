using ContaMente.Models;
using ContaMente.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TipoPagamentoController : ControllerBase
    {
        private readonly ITipoPagamentoService _tipoPagamentoService;
        public TipoPagamentoController(ITipoPagamentoService tipoPagamentoService) => _tipoPagamentoService = tipoPagamentoService;
        
        [HttpGet]
        public ActionResult<List<TipoPagamentoEnum>> GetTiposPagamento()
        {
            var tiposPagamento = _tipoPagamentoService.GetTiposPagamento();

            return Ok(tiposPagamento);
        }

        [HttpGet("{id}")]
        public ActionResult<TipoPagamentoEnum> GetTipoPagamentoById(int id)
        {
            var tipoPagamento = _tipoPagamentoService.GetTipoPagamentoById(id);

            if (tipoPagamento == null)
            {
                throw new KeyNotFoundException($"Tipo de pagamento com ID {id} não encontrado.");
            }

            return Ok(tipoPagamento);
        }
    }
}
