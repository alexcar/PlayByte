namespace PlayByte.Application.Abstractions.Notifications;

public sealed record EmailMessage(string To, string Subject, string Body);

/// <summary>Porta de envio de e-mail. Plugue aqui SendGrid/SMTP/SES na Infrastructure.</summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
