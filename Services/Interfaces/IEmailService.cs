namespace ContaMente.Services.Interfaces
{
    public interface IEmailService
    {
        public Task SendResetPasswordEmail(string email, string resetLink);
    }
}
