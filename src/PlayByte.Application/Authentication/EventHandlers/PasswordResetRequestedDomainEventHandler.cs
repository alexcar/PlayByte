using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Application.Abstractions.Notifications;
using PlayByte.Domain.Users.Events;

namespace PlayByte.Application.Authentication.EventHandlers;

internal sealed class PasswordResetRequestedDomainEventHandler(IEmailSender emailSender)
    : IDomainEventHandler<PasswordResetRequested>
{
    public async Task Handle(PasswordResetRequested notification, CancellationToken cancellationToken)
    {
        var link = $"https://app.playbyte.com/redefinir-senha?token={notification.Token}";
        var body =
            $"""
             Recebemos uma solicitacao para redefinir sua senha.
             Clique no link abaixo (valido por 30 minutos):

             {link}

             Se voce nao solicitou, ignore este e-mail.
             """;

        await emailSender.SendAsync(
            new EmailMessage(notification.Email, "PlayByte - Recuperacao de senha", body),
            cancellationToken);
    }
}
