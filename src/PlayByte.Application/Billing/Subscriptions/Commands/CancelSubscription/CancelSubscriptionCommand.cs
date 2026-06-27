using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Billing.Subscriptions.Commands.CancelSubscription;

public sealed record CancelSubscriptionCommand(Guid UserId) : ICommand;
