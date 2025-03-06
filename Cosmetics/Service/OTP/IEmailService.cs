namespace Cosmetics.Service.OTP
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
