using Microsoft.AspNetCore.Mvc;

namespace ContaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartaoController : ControllerBase
    {
        private readonly ICartaoService _cartaoService;

        public CartaoController(ICartaoService cartaoService) => _cartaoService = cartaoService;

        [HttpGet]
        public async Task<IActionResult> GetCartoes()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var cartoes = await _cartaoService.GetCartoes(userId!);

            return Ok(cartoes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCartaoById(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var cartao = await _cartaoService.GetCartaoById(id, userId!);

            if (cartao == null)
            {
                throw new KeyNotFoundException($"Cartão com ID {id} não encontrado.");
            }

            return Ok(cartao);
        }

        [HttpPost]
        public async Task<ActionResult<Cartao>> CreateCartao([FromBody] CreateUpdateCartaoDto createCartaoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var cartao = await _cartaoService.CreateCartao(createCartaoDto, userId!);

            return CreatedAtAction(nameof(GetCartaoById), new { id = cartao.Id }, cartao);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCartao(int id, [FromBody] CreateUpdateCartaoDto updateCartaoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var cartao = await _cartaoService.UpdateCartao(id, updateCartaoDto, userId!);

            if (cartao == null)
            {
                throw new KeyNotFoundException($"Cartão com ID {id} não encontrado.");
            }

            return Ok(cartao);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartao(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var result = await _cartaoService.DeleteCartao(id, userId!);

            if (!result)
            {
                throw new KeyNotFoundException($"Cartão com ID {id} não encontrado.");
            }

            return NoContent();
        }
    }
}
