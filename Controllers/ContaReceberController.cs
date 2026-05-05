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
    public class ContaReceberController : ControllerBase
    {
        private readonly IContaReceberService _contaReceberService;

        public ContaReceberController(IContaReceberService contaReceberService)
        {
            _contaReceberService = contaReceberService;
        }

        [HttpGet]
        public async Task<IActionResult> GetContasReceber(
            [FromQuery] int[]? categoriasIds,
            [FromQuery] int[]? responsaveisIds,
            [FromQuery] DateTime? dataVencimentoInicio,
            [FromQuery] DateTime? dataVencimentoFim,
            [FromQuery] bool? pago,
            [FromQuery] StatusDuplicataEnum? status)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var contas = await _contaReceberService.GetContasReceber(categoriasIds, responsaveisIds, dataVencimentoInicio, dataVencimentoFim, pago, status, userId!);

            return Ok(contas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContaReceberById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _contaReceberService.GetContaReceberById(id, userId!);

            if (conta == null)
            {
                throw new KeyNotFoundException($"Conta a Receber com ID {id} não encontrada.");
            }

            return Ok(conta);
        }

        [HttpPost]
        public async Task<IActionResult> LancarContasReceber([FromBody] CreateContaReceberDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var parcelas = await _contaReceberService.LancarContasReceber(dto, userId!);

            return CreatedAtAction(nameof(GetContasReceber), null, parcelas);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContaReceber(int id, [FromBody] UpdateContaReceberDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _contaReceberService.UpdateContaReceber(id, dto, userId!);

            if (conta == null)
            {
                throw new KeyNotFoundException($"Conta a Receber com ID {id} não encontrada.");
            }

            return Ok(conta);
        }

        [HttpPost("{id}/baixas")]
        public async Task<IActionResult> BaixarContaReceber(int id, [FromBody] BaixaContaReceberDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _contaReceberService.BaixarContaReceber(id, dto, userId!);

            if (conta == null)
            {
                throw new KeyNotFoundException($"Conta a Receber com ID {id} não encontrada.");
            }

            return Ok(conta);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContaReceber(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _contaReceberService.DeleteContaReceber(id, userId!);

            if (!result)
            {
                throw new KeyNotFoundException($"Conta a Receber com ID {id} não encontrada.");
            }

            return NoContent();
        }
    }
}
