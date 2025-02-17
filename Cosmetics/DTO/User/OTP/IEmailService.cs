namespace Cosmetics.DTO.User.OTP
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
