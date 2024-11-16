using ContaMente.Contexts;
using ContaMente.DTOs;
using ContaMente.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GastoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public GastoController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public IActionResult GetGastos()
        {
            var gastos = _context.Gastos.ToList();

            return Ok(gastos);
        }

        [HttpGet("{id}")]
        public IActionResult GetGastosById(int id)
        {
            var gasto = _context.Gastos.FirstOrDefault(g => g.Id == id);

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

            var gasto = new Gasto
            {
                Valor = createGastoDto.Valor,
                Descricao = createGastoDto.Descricao,
                Data = createGastoDto.Data,
                CategoriaId = createGastoDto.CategoriaId
            };
            
            _context.Gastos.Add(gasto);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetGastosById), new { id = gasto.Id }, gasto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGasto(int id, [FromBody] UpdateGastoDto updateGastoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var gasto = await _context.Gastos.FirstOrDefaultAsync(g => g.Id == id);

            if(gasto == null)
            {
                return NotFound();
            }

            if(updateGastoDto.Valor.HasValue)
            {
                gasto.Valor = updateGastoDto.Valor.Value;
            }

            if(updateGastoDto.Data.HasValue)
            {
                gasto.Data = updateGastoDto.Data.Value;
            }

            if(!string.IsNullOrEmpty(updateGastoDto.Descricao))
            {
                gasto.Descricao = updateGastoDto.Descricao;
            }

            if (updateGastoDto.CategoriaId.HasValue)
            {
                gasto.CategoriaId = updateGastoDto.CategoriaId.Value;
            }
            
            await _context.SaveChangesAsync();

            return Ok(gasto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGasto(int id)
        {
            var gasto = await _context.Gastos.FirstOrDefaultAsync(g => g.Id == id);

            if (gasto == null)
            {
                return NotFound();
            }
            
            _context.Gastos.Remove(gasto);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
