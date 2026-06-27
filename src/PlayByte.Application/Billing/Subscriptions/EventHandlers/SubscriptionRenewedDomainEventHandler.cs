using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Application.Abstractions.Notifications;
using PlayByte.Domain.Billing.Events;
using PlayByte.Domain.Users;

namespace PlayByte.Application.Billing.Subscriptions.EventHandlers;

internal sealed class SubscriptionRenewedDomainEventHandler(
    IUserRepository users, IEmailSender emailSender)
    : IDomainEventHandler<SubscriptionRenewed>
{
    public async Task Handle(SubscriptionRenewed e, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(e.UserId, cancellationToken);
        if (user is null) return;

        var body =
            $"""
             Sua assinatura PlayByte foi renovada.

             Plano: {e.PlanName}
             Valor cobrado: {e.Amount:0.00} {e.Currency}
             Numero da transacao: {e.PaymentId}
             Proxima cobranca: {e.NextBillingDate:dd/MM/yyyy}
             """;

        await emailSender.SendAsync(
            new EmailMessage(user.Email.Value, "PlayByte - Confirmacao de renovacao", body), cancellationToken);
    }
}
