using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace HomeWork_65.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(
        string email,
        string subject,
        string message)
    {
        var settings = _configuration.GetSection("EmailSettings");

        string host = settings["Host"]
                      ?? throw new InvalidOperationException(
                          "Не указан SMTP-сервер.");

        int port = settings.GetValue<int>("Port");

        string senderEmail = settings["Email"]
                             ?? throw new InvalidOperationException(
                                 "Не указан email отправителя.");

        string password = settings["Password"]
                          ?? throw new InvalidOperationException(
                              "Не указан пароль приложения.");

        string senderName = settings["SenderName"]
                            ?? "Администрация сайта";

        var emailMessage = new MimeMessage();

        emailMessage.From.Add(
            new MailboxAddress(senderName, senderEmail));

        emailMessage.To.Add(
            new MailboxAddress("", email));

        emailMessage.Subject = subject;

        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = message
        };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            host,
            port,
            SecureSocketOptions.SslOnConnect);

        await client.AuthenticateAsync(senderEmail, password);

        await client.SendAsync(emailMessage);

        await client.DisconnectAsync(true);
    }
}