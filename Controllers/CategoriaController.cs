using ContaMente.Contexts;
using ContaMente.DTOs;
using ContaMente.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriaController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public IActionResult GetCategorias()
        {
            var categorias = _context.Categorias.Include(c => c.Gastos).ToList();

            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public IActionResult GetCategoriasById(int id)
        {
            var categoria = _context.Categorias.FirstOrDefault(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            return Ok(categoria);
        }

        [HttpPost]
        public async Task<ActionResult<Gasto>> CreateCategoria([FromBody] CreateCategoriaDto createCategoriaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoria = new Categoria
            {
                Nome = createCategoriaDto.Nome
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoriasById), new { id = categoria.Id }, categoria);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategoria(int id, [FromBody] UpdateCategoriaDto updateCategoriaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            categoria.Nome = updateCategoriaDto.Nome;

            await _context.SaveChangesAsync();

            return Ok(categoria);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
