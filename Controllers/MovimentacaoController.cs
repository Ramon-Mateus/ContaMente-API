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
            [FromQuery] bool entrada,
            [FromQuery] List<int> categoriasIds,
            [FromQuery] List<int> tiposPagamentoIds,
            [FromQuery] List<int> responsaveisIds,
            [FromQuery] List<int> cartoesIds)
        {
            if (mes < 1 || mes > 12)
                return BadRequest("O mês deve estar entre 1 e 12.");

            if (ano < 1)
                return BadRequest("Ano inválido.");

            if (!mes.HasValue || !ano.HasValue)
                return BadRequest("Mês ou ano não especificado.");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var movimentacoes = await _movimentacaoService
                .GetMovimentacoes(
                    mes, 
                    ano, 
                    userId, 
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

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var movimentacao = await _movimentacaoService.GetMovimentacaoById(id, userId);

            if (movimentacao == null)
            {
                return NotFound();
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

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var categoria = await _categoriaService.GetCategoriaById(createMovimentacaoDto.CategoriaId, userId);

            if (categoria == null)
            {
                return BadRequest("Categoria não encontrada ou não pertence ao usuário.");
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

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var movimentacao = await _movimentacaoService.UpdateMovimentacao(id, updateMovimentacaoDto, userId);

            if (movimentacao == null)
            {
                return NotFound("Gasto não encontrado ou não pertence ao usuário.");
            }

            return Ok(movimentacao);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovimentacao(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var result = await _movimentacaoService.DeleteMovimentacao(id, userId);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
