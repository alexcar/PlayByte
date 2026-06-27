using PlayByte.Domain.Abstractions;
using PlayByte.Domain.Billing.Enumerations;
using PlayByte.Domain.Billing.Events;
using PlayByte.Domain.Billing.ValueObjects;
using PlayByte.Domain.Common;
using PlayByte.Domain.Users;

namespace PlayByte.Domain.Billing;

public sealed class Payment : AggregateRoot<PaymentId>, IAuditable
{
    private Payment() { } // EF Core

    private Payment(PaymentId id, SubscriptionId subscriptionId, UserId userId,
        Money amount, PaymentMethod method) : base(id)
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        Amount = amount;
        Method = method;
        Status = PaymentStatus.Pending;
    }

    public SubscriptionId SubscriptionId { get; private set; } // referencia por ID (agregado separado)
    public UserId UserId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public PaymentMethod Method { get; private set; } = default!;
    public PaymentStatus Status { get; private set; } = default!;
    public string? GatewayTransactionId { get; private set; }
    public DateTimeOffset? ProcessedAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static Result<Payment> Initiate(
        SubscriptionId subscriptionId, UserId userId, Money amount, PaymentMethod method)
    {
        if (!amount.IsPositive)
            return PaymentErrors.NonPositiveAmount;

        var payment = new Payment(PaymentId.New(), subscriptionId, userId, amount, method);
        payment.RaiseDomainEvent(new PaymentInitiated(payment.Id, subscriptionId, amount.Amount, amount.Currency));
        return payment;
    }

    public Result Approve(string gatewayTransactionId, DateTimeOffset nowUtc)
    {
        if (Status == PaymentStatus.Approved) return Result.Success(); // idempotente (webhook reentrega)
        if (Status != PaymentStatus.Pending) return PaymentErrors.NotPending;
        if (string.IsNullOrWhiteSpace(gatewayTransactionId)) return PaymentErrors.MissingGatewayId;

        Status = PaymentStatus.Approved;
        GatewayTransactionId = gatewayTransactionId;
        ProcessedAtUtc = nowUtc;
        RaiseDomainEvent(new PaymentApproved(Id, SubscriptionId, UserId));
        return Result.Success();
    }

    public Result Decline(string reason, DateTimeOffset nowUtc)
    {
        if (Status == PaymentStatus.Declined) return Result.Success();
        if (Status != PaymentStatus.Pending) return PaymentErrors.NotPending;

        Status = PaymentStatus.Declined;
        ProcessedAtUtc = nowUtc;
        RaiseDomainEvent(new PaymentDeclined(Id, SubscriptionId, UserId, reason));
        return Result.Success();
    }

    public Result Refund(DateTimeOffset nowUtc)
    {
        if (Status != PaymentStatus.Approved) return PaymentErrors.OnlyApprovedCanBeRefunded;

        Status = PaymentStatus.Refunded;
        ProcessedAtUtc = nowUtc;
        RaiseDomainEvent(new PaymentRefunded(Id, SubscriptionId, UserId, Amount.Amount, Amount.Currency));
        return Result.Success();
    }
}
