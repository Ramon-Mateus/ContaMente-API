using System.Net.Mail;
using System.Net;
using ContaMente.Services.Interfaces;

namespace ContaMente.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendResetPasswordEmail(string email, string resetLink)
        {
            string smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST")!;
            string smtpPort = Environment.GetEnvironmentVariable("SMTP_PORT")!;
            string smtpUser = Environment.GetEnvironmentVariable("SMTP_USER")!;
            string smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD")!;

            var client = new SmtpClient(smtpHost)
            {
                Port = int.Parse(smtpPort),
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser),
                Subject = "Recuperação de Senha",
                Body = $"Clique no link para redefinir sua senha: {resetLink}",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
    }
}
