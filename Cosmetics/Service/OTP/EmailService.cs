using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging; // Thêm logging
using System.Threading.Tasks;

namespace Cosmetics.Service.OTP
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpEmail;
        private readonly string _smtpPassword;
        private readonly ILogger<EmailService> _logger; // Thêm ILogger

        public EmailService(string smtpServer, int smtpPort, string smtpEmail, string smtpPassword, ILogger<EmailService> logger)
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpEmail = smtpEmail;
            _smtpPassword = smtpPassword;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Comedics", _smtpEmail));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain")
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                try
                {
                    _logger.LogInformation("Connecting to SMTP server {Server}:{Port}", _smtpServer, _smtpPort);
                    await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.Auto);

                    _logger.LogInformation("Authenticating with SMTP server for {Email}", _smtpEmail);
                    await client.AuthenticateAsync(_smtpEmail, _smtpPassword);

                    _logger.LogInformation("Sending email to {Recipient}", email);
                    await client.SendAsync(emailMessage);

                    _logger.LogInformation("Email sent successfully to {Recipient}", email);
                }
                catch (AuthenticationException authEx)
                {
                    _logger.LogError(authEx, "Authentication failed for {Email}", _smtpEmail);
                    throw new Exception("Failed to authenticate with SMTP server. Check your email or password.", authEx);
                }
                catch (SmtpCommandException smtpEx)
                {
                    _logger.LogError(smtpEx, "SMTP command failed while sending email to {Recipient}", email);
                    throw new Exception("SMTP command failed. Check SMTP server configuration.", smtpEx);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while sending email to {Recipient}", email);
                    throw;
                }
                finally
                {
                    if (client.IsConnected)
                    {
                        await client.DisconnectAsync(true);
                        _logger.LogInformation("Disconnected from SMTP server");
                    }
                }
            }
        }
    }
}