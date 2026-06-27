using PlayByte.Domain.Abstractions;
using PlayByte.Domain.Users;

namespace PlayByte.Domain.Billing.Events;

public sealed record PaymentInitiated(PaymentId PaymentId, SubscriptionId SubscriptionId, decimal Amount, string Currency) : IDomainEvent;
public sealed record PaymentApproved(PaymentId PaymentId, SubscriptionId SubscriptionId, UserId UserId) : IDomainEvent;
public sealed record PaymentDeclined(PaymentId PaymentId, SubscriptionId SubscriptionId, UserId UserId, string Reason) : IDomainEvent;
public sealed record PaymentRefunded(PaymentId PaymentId, SubscriptionId SubscriptionId, UserId UserId, decimal Amount, string Currency) : IDomainEvent;
