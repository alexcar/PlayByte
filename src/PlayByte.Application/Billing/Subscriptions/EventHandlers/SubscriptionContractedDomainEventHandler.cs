using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Application.Abstractions.Notifications;
using PlayByte.Domain.Billing.Events;
using PlayByte.Domain.Users;

namespace PlayByte.Application.Billing.Subscriptions.EventHandlers;

internal sealed class SubscriptionContractedDomainEventHandler(
    IUserRepository users, IEmailSender emailSender)
    : IDomainEventHandler<SubscriptionContracted>
{
    public async Task Handle(SubscriptionContracted e, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(e.UserId, cancellationToken);
        if (user is null) return;

        var body =
            $"""
             Bem-vindo(a) ao PlayByte Premium!

             Plano: {e.PlanName}
             Valor cobrado: {e.Amount:0.00} {e.Currency}
             Numero da transacao: {e.PaymentId}
             Proxima cobranca: {e.NextBillingDate:dd/MM/yyyy}

             Sua reproducao de musicas ja esta liberada. Bom som!
             """;

        await emailSender.SendAsync(
            new EmailMessage(user.Email.Value, "PlayByte - Confirmacao de contratacao", body), cancellationToken);
    }
}
