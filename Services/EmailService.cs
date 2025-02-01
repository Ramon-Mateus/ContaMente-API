using System.Net.Mail;
using System.Net;
using ContaMente.Services.Interfaces;

namespace ContaMente.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendResetPasswordEmail(string email, string resetLink)
        {
            var client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("ramonmateus00@gmail.com", ""),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("ramonmateus00@gmail.com"),
                Subject = "Recuperação de Senha",
                Body = $"Clique no link para redefinir sua senha: {resetLink}",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
    }
}
