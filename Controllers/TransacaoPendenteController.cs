using System.Security.Claims;
using ContaMente.DTOs;
using ContaMente.Services.Pluggy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransacaoPendenteController : ControllerBase
    {
        private readonly ITransacaoPendenteService _service;

        public TransacaoPendenteController(ITransacaoPendenteService service) => _service = service;

        private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] int status = 0)
        {
            if (!Enum.IsDefined(typeof(StatusTransacaoPendente), status))
                throw new ArgumentException("Status inválido.");

            return Ok(await _service.Listar(UserId, (StatusTransacaoPendente)status));
        }

        [HttpGet("contagem")]
        public async Task<IActionResult> Contagem() => Ok(new { pendentes = await _service.ContarPendentes(UserId) });

        [HttpPost("{id:int}/aprovar")]
        public async Task<IActionResult> Aprovar(int id, [FromBody] CreateMovimentacaoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var movimentacao = await _service.Aprovar(id, dto, UserId);
            return Ok(new { movimentacaoId = movimentacao.Id });
        }

        [HttpPost("{id:int}/aprovar-parcelada")]
        public async Task<IActionResult> AprovarParcelada(int id, [FromBody] CreateParcelaDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var parcela = await _service.AprovarParcelada(id, dto, UserId);
            return Ok(new { parcelaId = parcela.Id });
        }

        [HttpPost("{id:int}/ignorar")]
        public async Task<IActionResult> Ignorar(int id)
        {
            await _service.AlterarStatus(id, StatusTransacaoPendente.Ignorada, UserId);
            return NoContent();
        }

        [HttpPost("{id:int}/restaurar")]
        public async Task<IActionResult> Restaurar(int id)
        {
            await _service.AlterarStatus(id, StatusTransacaoPendente.Pendente, UserId);
            return NoContent();
        }
    }
}
