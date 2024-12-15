using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ContaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GastoController : ControllerBase
    {
        private readonly IGastoService _gastoService;

        public GastoController(IGastoService gastoService) => _gastoService = gastoService;

        [HttpGet]
        public async Task<IActionResult> GetGastos([FromQuery] int? mes, [FromQuery] int? ano)
        {
            if (mes < 1 || mes > 12)
                return BadRequest("O mês deve estar entre 1 e 12.");

            if (ano < 1)
                return BadRequest("Ano inválido.");
            
            if(!mes.HasValue || !ano.HasValue)
                return BadRequest("Mês ou ano não especificado.");
            
            var gastos = await _gastoService.GetGastos(mes, ano);

            return Ok(gastos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGastoById(int id)
        {
            var gasto = await _gastoService.GetGastoById(id);

            if (gasto == null)
            {
                return NotFound();
            }
            
            return Ok(gasto);
        }

        [HttpPost]
        public async Task<ActionResult<Gasto>> CreateGasto([FromBody] CreateGastoDto createGastoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var gasto = await _gastoService.CreateGasto(createGastoDto);

            return CreatedAtAction(nameof(GetGastoById), new { id = gasto.Id }, gasto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGasto(int id, [FromBody] UpdateGastoDto updateGastoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var gasto = await _gastoService.UpdateGasto(id, updateGastoDto);

            if (gasto == null)
            {
                return NotFound();
            }

            return Ok(gasto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGasto(int id)
        {
            var result = await _gastoService.DeleteGasto(id);

            if (!result)
            {
                return NotFound();
            }
            
            return NoContent();
        }
    }
}
