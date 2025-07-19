using EducationalGames.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EducationalGames.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder();
            if (isHtml)
                builder.HtmlBody = body;
            else
                builder.TextBody = body;

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {EmailAddress}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {EmailAddress}", to);
            throw;
        }
    }

    public async Task SendEmailConfirmationAsync(string to, string confirmationLink)
    {
        var subject = "Conferma il tuo indirizzo email";
        var body = $@"
            <h1>Benvenuto in Educational Games!</h1>
            <p>Per confermare il tuo indirizzo email, clicca sul link seguente:</p>
            <p><a href='{confirmationLink}'>Conferma Email</a></p>
            <p>Se non hai richiesto questa email, puoi ignorarla.</p>";

        await SendEmailAsync(to, subject, body, isHtml: true);
    }

    public async Task SendPasswordResetAsync(string to, string resetLink)
    {
        var subject = "Reset Password";
        var body = $@"
            <h1>Reset della Password</h1>
            <p>Per reimpostare la tua password, clicca sul link seguente:</p>
            <p><a href='{resetLink}'>Reset Password</a></p>
            <p>Se non hai richiesto il reset della password, ignora questa email.</p>";

        await SendEmailAsync(to, subject, body, isHtml: true);
    }
}
