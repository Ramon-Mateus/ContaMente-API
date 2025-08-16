using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ContaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponsavelController : ControllerBase
    {
        private readonly IResponsavelService _responsavelService;

        public ResponsavelController(IResponsavelService responsavelService) => _responsavelService = responsavelService;

        [HttpGet]
        public async Task<IActionResult> GetResponsaveis()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var responsaveis = await _responsavelService.GetResponsaveis(userId);

            return Ok(responsaveis);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetResponsavelById(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var responsavel = await _responsavelService.GetResponsavelById(id, userId);

            if (responsavel == null)
            {
                return NotFound();
            }

            return Ok(responsavel);
        }

        [HttpPost]
        public async Task<ActionResult<Categoria>> CreateResponsavel([FromBody] CreateUpdateResponsavelDto createResponsavelDto)
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

            var responsavel = await _responsavelService.CreateResponsavel(createResponsavelDto, userId);

            return CreatedAtAction(nameof(GetResponsavelById), new { id = responsavel.Id }, responsavel);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResponsavel(int id, [FromBody] CreateUpdateResponsavelDto updateResponsavelDto)
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

            var responsavel = await _responsavelService.UpdateResponsavel(id, updateResponsavelDto, userId);

            if (responsavel == null)
            {
                return NotFound();
            }

            return Ok(responsavel);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResponsavel(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var result = await _responsavelService.DeleteResponsavel(id, userId);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
