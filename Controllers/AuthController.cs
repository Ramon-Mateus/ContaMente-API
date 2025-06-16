using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Services.Interfaces;

namespace ContaMente.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;

        private readonly UserManager<User> _userManager;

        private readonly IEmailService _emailService;

        public AuthController(SignInManager<User> signInManager, UserManager<User> userManager, IEmailService emailService)
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

            string rotaBase = Environment.GetEnvironmentVariable("ROTA_BASE")!;

            var resetLink = $"{rotaBase}/resetPassword?email={user.Email}&token={Uri.EscapeDataString(token)}";

            await _emailService.SendResetPasswordEmail(user.Email!, resetLink);

            return Ok(new { message = "Link de redefinição enviado para o email." });
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

            return Ok(new { message = "Senha redefinida com sucesso." });
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new
            {
                Success = true,
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email
            });
        }

        [HttpGet("getUser")]
        public async Task<IActionResult> getUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email
            });
        }
    }
}
