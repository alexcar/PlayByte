using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Application.Abstractions.Notifications;
using PlayByte.Domain.Billing.Events;
using PlayByte.Domain.Users;

namespace PlayByte.Application.Billing.Subscriptions.EventHandlers;

internal sealed class SubscriptionCanceledDomainEventHandler(
    IUserRepository users, IEmailSender emailSender)
    : IDomainEventHandler<SubscriptionCanceled>
{
    public async Task Handle(SubscriptionCanceled e, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(e.UserId, cancellationToken);
        if (user is null) return;

        var body =
            $"""
             Sua assinatura PlayByte ({e.PlanName}) foi cancelada.

             Voce continua com acesso ate {e.AccessUntil:dd/MM/yyyy}.
             Nenhuma cobranca futura sera realizada.
             """;

        await emailSender.SendAsync(
            new EmailMessage(user.Email.Value, "PlayByte - Confirmacao de cancelamento", body), cancellationToken);
    }
}
