using Api.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Api.Services;

public class EmailSender : IEmailSender
{
    private readonly MailOptions _mailOptions;

    public EmailSender(IOptions<MailOptions> mailOptions)
    {
        _mailOptions = mailOptions.Value;
    }

    public async Task SendMail(string email, string login, string name, string password)
    {
        using var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress("Dex", _mailOptions.Address));
        emailMessage.To.Add(new MailboxAddress(name, email));
        emailMessage.Subject = "Ivitation";
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            // TODO Здесь по идее какая-то html'а 
            Text = $"Hello there {name}, here is your shit {login} - {password}"
        };

        using var smtpClient = new SmtpClient();

        await smtpClient.ConnectAsync(_mailOptions.SmtpType, _mailOptions.SmtpPort, false);
        await smtpClient.AuthenticateAsync(_mailOptions.Address, _mailOptions.Password);
        await smtpClient.SendAsync(emailMessage);
        await smtpClient.DisconnectAsync(true);
    }
}