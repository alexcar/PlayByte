using PlayByte.Domain.Abstractions;
using PlayByte.Domain.Users;

namespace PlayByte.Domain.Billing.Events;

public sealed record SubscriptionStarted(SubscriptionId SubscriptionId, UserId UserId, string PlanName, string Status) : IDomainEvent;

// Primeiro pagamento aprovado (boas-vindas / contratacao - US-06)
public sealed record SubscriptionContracted(
    SubscriptionId SubscriptionId, UserId UserId, string PlanName,
    decimal Amount, string Currency, DateOnly NextBillingDate, PaymentId PaymentId) : IDomainEvent;

// Pagamentos subsequentes (renovacao - US-07)
public sealed record SubscriptionRenewed(
    SubscriptionId SubscriptionId, UserId UserId, string PlanName,
    decimal Amount, string Currency, DateOnly NextBillingDate, PaymentId PaymentId) : IDomainEvent;

public sealed record SubscriptionPastDue(SubscriptionId SubscriptionId, UserId UserId) : IDomainEvent;

// Cancelamento (US-08)
public sealed record SubscriptionCanceled(
    SubscriptionId SubscriptionId, UserId UserId, string PlanName, DateOnly AccessUntil) : IDomainEvent;

public sealed record SubscriptionExpired(SubscriptionId SubscriptionId, UserId UserId) : IDomainEvent;
