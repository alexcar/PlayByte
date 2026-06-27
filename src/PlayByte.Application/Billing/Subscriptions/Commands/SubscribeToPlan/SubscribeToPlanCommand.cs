using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Billing.Subscriptions.Commands.SubscribeToPlan;

public sealed record SubscribeToPlanCommand(Guid UserId, string PlanCode, string PaymentMethod)
    : ICommand<SubscribeToPlanResponse>;

public sealed record SubscribeToPlanResponse(Guid SubscriptionId, Guid PaymentId);
