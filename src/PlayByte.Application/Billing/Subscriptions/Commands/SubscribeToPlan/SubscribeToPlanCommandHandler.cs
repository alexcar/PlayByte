using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Application.Billing.Plans;
using PlayByte.Domain.Billing;
using PlayByte.Domain.Billing.Enumerations;
using PlayByte.Domain.Billing.ValueObjects;
using PlayByte.Domain.Common;
using PlayByte.Domain.Users;

namespace PlayByte.Application.Billing.Subscriptions.Commands.SubscribeToPlan;

/// <summary>
/// Cria a assinatura em PendingPayment + um pagamento Pending. O acesso so e' liberado
/// quando o webhook aprovar o pagamento (US-05 c2/c4).
/// </summary>
internal sealed class SubscribeToPlanCommandHandler(
    ISubscriptionRepository subscriptions,
    IPaymentRepository payments,
    IApplicationDbContext dbContext,
    TimeProvider timeProvider)
    : ICommandHandler<SubscribeToPlanCommand, SubscribeToPlanResponse>
{
    public async Task<Result<SubscribeToPlanResponse>> Handle(
        SubscribeToPlanCommand command, CancellationToken cancellationToken)
    {
        var definition = PlanCatalog.Find(command.PlanCode);
        if (definition is null)
            return BillingApplicationErrors.PlanNotFound;

        var method = ResolveMethod(command.PaymentMethod);
        if (method is null)
            return BillingApplicationErrors.InvalidPaymentMethod;

        var priceResult = Money.Create(definition.Price, definition.Currency);
        if (priceResult.IsFailure) return priceResult.Error;

        var planResult = Plan.Create(definition.Name, priceResult.Value, definition.Interval, definition.TrialDays);
        if (planResult.IsFailure) return planResult.Error;

        var userId = new UserId(command.UserId);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var subResult = Subscription.StartPendingPayment(userId, planResult.Value, today);
        if (subResult.IsFailure) return subResult.Error;

        var paymentResult = Payment.Initiate(subResult.Value.Id, userId, priceResult.Value, method);
        if (paymentResult.IsFailure) return paymentResult.Error;

        subscriptions.Add(subResult.Value);
        payments.Add(paymentResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new SubscribeToPlanResponse(subResult.Value.Id.Value, paymentResult.Value.Id.Value);
    }

    private static PaymentMethod? ResolveMethod(string code) => code.ToLowerInvariant() switch
    {
        "credit_card" => PaymentMethod.CreditCard,
        "debit_card"  => PaymentMethod.DebitCard,
        "pix"         => PaymentMethod.Pix,
        "boleto"      => PaymentMethod.Boleto,
        _ => null
    };
}
