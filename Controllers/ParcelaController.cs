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
        private readonly IMovimentacaoParcelaService _movimentacaoParcelaService;

        public ParcelaController(IParcelaService parcelaService, IMovimentacaoParcelaService movimentacaoParcelaService)
        {
            _parcelaService = parcelaService;
            _movimentacaoParcelaService = movimentacaoParcelaService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Parcela>>> GetParcelas()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            return await _parcelaService.GetParcelas(userId!);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Parcela>> GetParcelaById(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var parcela = await _parcelaService.GetParcelaById(id, userId!);

            if (parcela == null)
            {
                throw new KeyNotFoundException($"Parcela com ID {id} não encontrado.");
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

            var parcela = await _parcelaService.CreateParcela(createParcelaDto, userId);

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

            var parcela = await _parcelaService.UpdateParcela(id, createParcelaDto, userId!);

            if (parcela == null)
            {
                throw new KeyNotFoundException($"Parcela com ID {id} não encontrada.");
            }

            return Ok(parcela);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteParcela(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var result = await _movimentacaoParcelaService.DeleteParcela(id, userId!);

            if (!result)
            {
                throw new KeyNotFoundException($"Parcela com ID {id} não encontrada.");
            }

            return Ok();
        }
    }
}
