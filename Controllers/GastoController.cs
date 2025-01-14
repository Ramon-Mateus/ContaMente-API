using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GastoController : ControllerBase
    {
        private readonly IGastoService _gastoService;
        private readonly ICategoriaService _categoriaService;

        public GastoController(IGastoService gastoService, ICategoriaService categoriaService)
        {
            _gastoService = gastoService;
            _categoriaService = categoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetGastos([FromQuery] int? mes, [FromQuery] int? ano)
        {
            if (mes < 1 || mes > 12)
                return BadRequest("O mês deve estar entre 1 e 12.");

            if (ano < 1)
                return BadRequest("Ano inválido.");
            
            if(!mes.HasValue || !ano.HasValue)
                return BadRequest("Mês ou ano não especificado.");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var gastos = await _gastoService.GetGastos(mes, ano, userId);

            return Ok(gastos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGastoById(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var gasto = await _gastoService.GetGastoById(id, userId);

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

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var categoria = await _categoriaService.GetCategoriaById(createGastoDto.CategoriaId, userId);

            if(categoria == null)
            {
                return BadRequest("Categoria não encontrada ou não pertence ao usuário.");
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

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var gasto = await _gastoService.UpdateGasto(id, updateGastoDto, userId);

            if (gasto == null)
            {
                return NotFound("Gasto não encontrado ou não pertence ao usuário.");
            }

            return Ok(gasto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGasto(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var result = await _gastoService.DeleteGasto(id, userId);

            if (!result)
            {
                return NotFound();
            }
            
            return NoContent();
        }
    }
}
