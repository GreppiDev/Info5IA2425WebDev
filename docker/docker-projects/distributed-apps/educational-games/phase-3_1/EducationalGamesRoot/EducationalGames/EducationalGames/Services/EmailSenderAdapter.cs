using Microsoft.AspNetCore.Identity.UI.Services;

namespace EducationalGames.Services;

//questa classe funge da adattatore per il servizio di invio email
//serve solo nel caso in cui si voglia configurare il servizio di invio email con Identity UI
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
