using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ParcelaController : ControllerBase
    {
        private readonly IParcelaService _parcelaService;

        public ParcelaController(IParcelaService parcelaService)
        {
            _parcelaService = parcelaService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Parcela>>> GetParcelas()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            return await _parcelaService.GetParcelas(userId);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Parcela>> GetParcelaById(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var parcela = await _parcelaService.GetParcelaById(id, userId);

            if (parcela == null)
            {
                return NotFound();
            }

            return Ok(parcela);
        }

        [HttpPost]
        public async Task<ActionResult<Parcela>> CreateParcela([FromBody] CreateParcelaDto createParcelaDto)
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

            var parcela = await _parcelaService.CreateParcela(createParcelaDto);

            return CreatedAtAction(nameof(GetParcelaById), new { id = parcela.Id }, parcela);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Parcela>> UpdateParcela(int id, [FromBody] CreateParcelaDto createParcelaDto)
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

            var parcela = await _parcelaService.UpdateParcela(id, createParcelaDto, userId);

            if (parcela == null)
            {
                return NotFound();
            }

            return Ok(parcela);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteParcela(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var result = await _parcelaService.DeleteParcela(id, userId);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
