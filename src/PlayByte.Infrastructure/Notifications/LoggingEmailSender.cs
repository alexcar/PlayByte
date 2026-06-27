using Microsoft.Extensions.Logging;
using PlayByte.Application.Abstractions.Notifications;

namespace PlayByte.Infrastructure.Notifications;

/// <summary>
/// Implementacao stand-in: registra o e-mail no log em vez de enviar de verdade.
/// Troque por um provedor real (SMTP/SendGrid/SES) implementando IEmailSender.
/// O retry de envio e' garantido pelo Outbox (eventos de notificacao).
/// </summary>
internal sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[EMAIL] Para={To} | Assunto={Subject}\n{Body}",
            message.To, message.Subject, message.Body);
        return Task.CompletedTask;
    }
}
