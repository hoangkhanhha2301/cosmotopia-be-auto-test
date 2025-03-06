using MailKit.Security;
using MimeKit;
using System.Net.Mail;
using MailKit.Net.Smtp;
using System.Threading.Tasks;

namespace Cosmetics.Service.OTP
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpEmail;
        private readonly string _smtpPassword;

        public EmailService(string smtpServer, int smtpPort, string smtpEmail, string smtpPassword)
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpEmail = smtpEmail;
            _smtpPassword = smtpPassword;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Comedics", _smtpEmail));
            emailMessage.To.Add(new MailboxAddress("", email)); // Tên người nhận có thể để trống hoặc là tên người dùng

            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain")
            {
                Text = message
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient()) // Sử dụng tên đầy đủ
            {
                try
                {
                    await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.Auto);
                    await client.AuthenticateAsync(_smtpEmail, _smtpPassword);
                    await client.SendAsync(emailMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email: {ex.Message}");
                    throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }

        }
    }
}
