using System.Net;
using System.Net.Mail;

namespace ShefaafAPI.Services;

public class MailService : IMailService
{
    private readonly IConfiguration config;

    public MailService(IConfiguration config)
    {
        this.config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml)
    {
        var smtpHost = config["Email:SmtpHost"];
        var smtpPort = int.Parse(config["Email:SmtpPort"]!);
        var smtpUser = config["Email:SmtpUser"];
        var smtpPass = config["Email:SmtpPass"];
        var fromEmail = config["Email:FromEmail"];

        using var message = new MailMessage();
        message.From = new MailAddress(fromEmail!);
        message.To.Add(to);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = isHtml;

        using var smtpClient = new SmtpClient(smtpHost, smtpPort);
        smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPass);
        smtpClient.EnableSsl = true;

        await smtpClient.SendMailAsync(message);
    }
}
