using ContaMente.DTOs;
using ContaMente.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContaPagarController : ControllerBase
    {
        private readonly IContaPagarService _contaPagarService;

        public ContaPagarController(IContaPagarService contaPagarService)
        {
            _contaPagarService = contaPagarService;
        }

        [HttpGet]
        public async Task<IActionResult> GetContasPagar(
            [FromQuery] int[]? categoriasIds, 
            [FromQuery] int[]? responsaveisIds, 
            [FromQuery] DateTime? dataVencimentoInicio, 
            [FromQuery] DateTime? dataVencimentoFim, 
            [FromQuery] bool? pago,
            [FromQuery] StatusDuplicataEnum? status)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var contas = await _contaPagarService.GetContasPagar(categoriasIds, responsaveisIds, dataVencimentoInicio, dataVencimentoFim, pago, status, userId!);

            return Ok(contas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContaPagarById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _contaPagarService.GetContaPagarById(id, userId!);

            if (conta == null)
            {
                throw new KeyNotFoundException($"Conta a Pagar com ID {id} não encontrada.");
            }

            return Ok(conta);
        }

        [HttpPost]
        public async Task<IActionResult> LancarContasPagar([FromBody] CreateContaPagarDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var parcelas = await _contaPagarService.LancarContasPagar(dto, userId!);

            return CreatedAtAction(nameof(GetContasPagar), null, parcelas);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContaPagar(int id, [FromBody] UpdateContaPagarDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _contaPagarService.UpdateContaPagar(id, dto, userId!);

            if (conta == null)
            {
                throw new KeyNotFoundException($"Conta a Pagar com ID {id} não encontrada.");
            }

            return Ok(conta);
        }

        [HttpPost("{id}/baixas")]
        public async Task<IActionResult> BaixarContaPagar(int id, [FromBody] BaixaContaPagarDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _contaPagarService.BaixarContaPagar(id, dto, userId!);

            if (conta == null)
            {
                throw new KeyNotFoundException($"Conta a Pagar com ID {id} não encontrada.");
            }

            return Ok(conta);
        }

        [HttpPut("{id}/pagar")]
        public async Task<IActionResult> MarcarComoPaga(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _contaPagarService.MarcarComoPaga(id, userId!);

            if (conta == null)
            {
                throw new KeyNotFoundException($"Conta a Pagar com ID {id} não encontrada.");
            }

            return Ok(conta);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContaPagar(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _contaPagarService.DeleteContaPagar(id, userId!);

            if (!result)
            {
                throw new KeyNotFoundException($"Conta a Pagar com ID {id} não encontrada.");
            }

            return NoContent();
        }
    }
}
