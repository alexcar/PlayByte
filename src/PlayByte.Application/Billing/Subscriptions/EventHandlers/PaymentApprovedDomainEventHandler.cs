using PlayByte.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using PlayByte.Application.Abstractions.Data;
using PlayByte.Domain.Billing;
using PlayByte.Domain.Billing.Events;

namespace PlayByte.Application.Billing.Subscriptions.EventHandlers;

/// <summary>
/// Orquestracao (consistencia eventual): pagamento aprovado -> ativa/renova a assinatura,
/// em transacao separada. Idempotente por PaymentId (seguro para retry do Outbox).
/// </summary>
internal sealed class PaymentApprovedDomainEventHandler(
    ISubscriptionRepository subscriptions,
    IApplicationDbContext dbContext,
    TimeProvider timeProvider,
    ILogger<PaymentApprovedDomainEventHandler> logger)
    : IDomainEventHandler<PaymentApproved>
{
    public async Task Handle(PaymentApproved notification, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.GetByIdAsync(notification.SubscriptionId, cancellationToken);
        if (subscription is null)
        {
            logger.LogWarning("PaymentApproved {PaymentId}: assinatura {SubscriptionId} nao encontrada.",
                notification.PaymentId, notification.SubscriptionId);
            return;
        }

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var result = subscription.RegisterApprovedPayment(notification.PaymentId, today);
        if (result.IsFailure)
        {
            logger.LogWarning("PaymentApproved {PaymentId}: falha ao aplicar em {SubscriptionId}: {Error}",
                notification.PaymentId, notification.SubscriptionId, result.Error.Code);
            return;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
