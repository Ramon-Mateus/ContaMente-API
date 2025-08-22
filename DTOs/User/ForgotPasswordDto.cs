using System.ComponentModel.DataAnnotations;

namespace ContaMente.DTOs
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
