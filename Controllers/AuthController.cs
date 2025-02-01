using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ContaMente.DTOs;
using ContaMente.Services.Interfaces;

namespace ContaMente.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        private readonly UserManager<IdentityUser> _userManager;

        private readonly IEmailService _emailService;

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IEmailService emailService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
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

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest("Usuário não encontrado.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = $"http://localhost:4200/resetPassword?email={user.Email}&token={Uri.EscapeDataString(token)}";

            await _emailService.SendResetPasswordEmail(user.Email!, resetLink);

            return Ok("Link de redefinição enviado para o seu email.");
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest("Usuário não encontrado.");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Senha redefinida com sucesso.");
        }
    }
}
