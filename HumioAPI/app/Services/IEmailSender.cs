namespace HumioAPI.Services;

public interface IEmailSender
{
    Task SendAsync(
        string toEmail,
        string subject,
        string body,
        bool isBodyHtml = false,
        CancellationToken cancellationToken = default);
}
