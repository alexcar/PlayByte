using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Billing.Subscriptions.Queries.GetMySubscription;

public sealed record GetMySubscriptionQuery(Guid UserId) : IQuery<MySubscriptionResponse>;

public sealed record MySubscriptionResponse(
    Guid SubscriptionId, string PlanName, string Status, DateOnly CurrentPeriodEnd, bool HasActiveAccess);
