namespace ShefaafAPI.Services;

public interface IMailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
}
