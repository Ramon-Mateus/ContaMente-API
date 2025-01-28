using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ContaMente.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public AuthController(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet("verificarSessao")]
        public IActionResult VerificarSessao()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return Ok(true);
            }
            return Ok(false);
        }
    }
}
