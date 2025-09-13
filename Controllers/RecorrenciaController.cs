using ContaMente.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

            await _recorrenciaService.CancelarRecorrencia(id, userId!);
            return Ok(new { message = "Recorrência cancelada com sucesso" });
        }
    }
}
