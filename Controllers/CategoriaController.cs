using ContaMente.Contexts;
using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriaController(ICategoriaService categoriaService) => _categoriaService = categoriaService;

        [HttpGet]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _categoriaService.GetCategorias();

            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoriaById(int id)
        {
            var categoria = await _categoriaService.GetCategoriaById(id);

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

            var categoria = await _categoriaService.CreateCategoria(createCategoriaDto);

            return CreatedAtAction(nameof(GetCategoriaById), new { id = categoria.Id }, categoria);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategoria(int id, [FromBody] UpdateCategoriaDto updateCategoriaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoria = await _categoriaService.UpdateCategoria(id, updateCategoriaDto);

            if (categoria == null)
            {
                return NotFound();
            }
            
            return Ok(categoria);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var result = await _categoriaService.DeleteCategoria(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
