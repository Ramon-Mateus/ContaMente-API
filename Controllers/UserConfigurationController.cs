using Microsoft.AspNetCore.Mvc;

namespace ContaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserConfigurationController : ControllerBase
    {
        private readonly IUserConfigurationService _userConfigurationService;

        public UserConfigurationController(IUserConfigurationService userConfigurationService) => _userConfigurationService = userConfigurationService;

        [HttpGet]
        public async Task<IActionResult> GetUserConfiguration()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var configuration = await _userConfigurationService.GetUserConfiguration(userId!);

            return Ok(configuration);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUserConfiguration([FromBody] UpdateUserConfigurationDto updateUserConfigurationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var configuration = await _userConfigurationService.UpdateUserConfiguration(updateUserConfigurationDto, userId!);

            if (configuration == null)
            {
                throw new KeyNotFoundException($"Configuração do usuário com ID {userId} não encontrada.");
            }

            return Ok(configuration);
        }
    }
}
