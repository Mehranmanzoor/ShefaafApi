namespace ShefaafAPI.Services;

public class MailService : IMailService
{
    private readonly ILogger<MailService> _logger;

    public MailService(ILogger<MailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        // For now, just log the email (you can implement SMTP later)
        _logger.LogInformation($"Email to: {to} | Subject: {subject} | Body: {body}");
        
        // TODO: Implement actual email sending with SMTP
        // For development, we'll just simulate success
        return Task.CompletedTask;
    }
}
