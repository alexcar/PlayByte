using PlayByte.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Notifications;
using PlayByte.Domain.Billing;
using PlayByte.Domain.Billing.Events;
using PlayByte.Domain.Users;

namespace PlayByte.Application.Billing.Subscriptions.EventHandlers;

/// <summary>
/// Pagamento recusado: se a assinatura estava ativa (renovacao), marca PastDue e envia
/// alerta para o usuario atualizar os dados (US-07 c3). Se era o 1o pagamento, a
/// assinatura permanece sem acesso (PendingPayment) - usuario segue no plano gratuito.
/// </summary>
internal sealed class PaymentDeclinedDomainEventHandler(
    ISubscriptionRepository subscriptions,
    IUserRepository users,
    IApplicationDbContext dbContext,
    IEmailSender emailSender,
    ILogger<PaymentDeclinedDomainEventHandler> logger)
    : IDomainEventHandler<PaymentDeclined>
{
    public async Task Handle(PaymentDeclined notification, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.GetByIdAsync(notification.SubscriptionId, cancellationToken);
        if (subscription is null)
            return;

        subscription.MarkPastDue(); // no-op se nao estava ativa
        await dbContext.SaveChangesAsync(cancellationToken);

        var user = await users.GetByIdAsync(notification.UserId, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("PaymentDeclined {PaymentId}: usuario {UserId} nao encontrado.",
                notification.PaymentId, notification.UserId);
            return;
        }

        var body =
            $"""
             Nao conseguimos processar o pagamento da sua assinatura PlayByte.
             Motivo informado: {notification.Reason}.

             Atualize seus dados de pagamento para evitar a interrupcao do acesso.
             """;

        await emailSender.SendAsync(
            new EmailMessage(user.Email.Value, "PlayByte - Falha no pagamento", body), cancellationToken);
    }
}
