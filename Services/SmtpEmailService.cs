using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Online_Restaurant.Configuration;

namespace Online_Restaurant.Services;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public SmtpEmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string body)
    {
        using var message = new MailMessage();

        message.From = new MailAddress(
            _settings.SenderEmail,
            _settings.SenderName);

        message.To.Add(to);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = true;

        using var smtpClient = new SmtpClient(
            _settings.SmtpServer,
            _settings.Port);

        smtpClient.Credentials = new NetworkCredential(
            _settings.Username,
            _settings.Password);

        smtpClient.EnableSsl = true;

        await smtpClient.SendMailAsync(message);
    }
}