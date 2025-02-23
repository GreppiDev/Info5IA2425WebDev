using Microsoft.AspNetCore.Identity.UI.Services;
using ProtectedAPI.Services;

namespace ProtectedAPI.Services;

public class EmailSenderAdapter : IEmailSender
{
    private readonly IEmailService _emailService;

    public EmailSenderAdapter(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await _emailService.SendEmailAsync(email, subject, htmlMessage, isHtml: true);
    }
}