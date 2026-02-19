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
    public class MovimentacaoController : ControllerBase
    {
        private readonly IMovimentacaoService _movimentacaoService;
        private readonly ICategoriaService _categoriaService;

        public MovimentacaoController(IMovimentacaoService movimentacaoService, ICategoriaService categoriaService)
        {
            _movimentacaoService = movimentacaoService;
            _categoriaService = categoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMovimentacoes(
            [FromQuery] int? mes, 
            [FromQuery] int? ano, 
            [FromQuery] bool? entrada,
            [FromQuery] List<int> categoriasIds,
            [FromQuery] List<int> tiposPagamentoIds,
            [FromQuery] List<int> responsaveisIds,
            [FromQuery] List<int> cartoesIds)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Isso chama o service
            var movimentacoes = await _movimentacaoService
                .GetMovimentacoes(
                    mes, 
                    ano, 
                    userId!, 
                    entrada, 
                    categoriasIds, 
                    tiposPagamentoIds,
                    responsaveisIds,
                    cartoesIds
                );

            return Ok(movimentacoes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovimentacaoById(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var movimentacao = await _movimentacaoService.GetMovimentacaoById(id, userId!);

            if (movimentacao == null)
            {
                throw new KeyNotFoundException($"Movimentação com ID {id} não encontrada.");
            }

            return Ok(movimentacao);
        }

        [HttpPost]
        public async Task<ActionResult<Movimentacao>> CreateMovimentacao([FromBody] CreateMovimentacaoDto createMovimentacaoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var categoria = await _categoriaService.GetCategoriaById(createMovimentacaoDto.CategoriaId, userId!);

            if (categoria == null)
            {
                throw new KeyNotFoundException("Categoria não encontrada.");
            }

            var movimentacao = await _movimentacaoService.CreateMovimentacao(createMovimentacaoDto);

            return CreatedAtAction(nameof(GetMovimentacaoById), new { id = movimentacao.Id }, movimentacao);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMovimentacao(int id, [FromBody] UpdateMovimentacaoDto updateMovimentacaoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var movimentacao = await _movimentacaoService.UpdateMovimentacao(id, updateMovimentacaoDto, userId!);

            if (movimentacao == null)
            {
                throw new KeyNotFoundException($"Movimentação com ID {id} não encontrada.");
            }

            return Ok(movimentacao);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovimentacao(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var result = await _movimentacaoService.DeleteMovimentacao(id, userId!);

            if (!result)
            {
                throw new KeyNotFoundException($"Movimentação com ID {id} não encontrada.");
            }

            return NoContent();
        }
    }
}
