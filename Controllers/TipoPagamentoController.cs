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
        public async Task<ActionResult<List<TipoPagamento>>> GetTiposPagamento()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var tiposPagamento = await _tipoPagamentoService.GetTiposPagamento();

            return Ok(tiposPagamento);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TipoPagamento>> GetTipoPagamentoById(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var tipoPagamento = await _tipoPagamentoService.GetTipoPagamentoById(id);

            if (tipoPagamento == null)
            {
                return NotFound();
            }

            return Ok(tipoPagamento);
        }
    }
}
