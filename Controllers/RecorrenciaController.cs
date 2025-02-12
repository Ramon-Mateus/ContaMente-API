using ContaMente.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecorrenciaController : ControllerBase
    {
        private readonly IRecorrenciaService _recorrenciaService;

        public RecorrenciaController(IRecorrenciaService recorrenciaService)
        {
            _recorrenciaService = recorrenciaService;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> CancelarRecorrencia(int id)
        {

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            try
            {
                await _recorrenciaService.CancelarRecorrencia(id, userId);
                return Ok(new { message = "Recorrência cancelada com sucesso" });
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
