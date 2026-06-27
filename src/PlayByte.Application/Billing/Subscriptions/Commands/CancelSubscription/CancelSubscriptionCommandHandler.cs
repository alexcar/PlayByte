using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Billing;
using PlayByte.Domain.Common;
using PlayByte.Domain.Users;

namespace PlayByte.Application.Billing.Subscriptions.Commands.CancelSubscription;

internal sealed class CancelSubscriptionCommandHandler(
    ISubscriptionRepository subscriptions,
    IApplicationDbContext dbContext,
    TimeProvider timeProvider)
    : ICommandHandler<CancelSubscriptionCommand>
{
    public async Task<Result> Handle(CancelSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.GetActiveByUserIdAsync(new UserId(command.UserId), cancellationToken);
        if (subscription is null)
            return SubscriptionErrors.NoActiveSubscription;

        var result = subscription.Cancel(timeProvider.GetUtcNow());
        if (result.IsFailure)
            return result.Error;

        // Acesso mantido ate o fim do periodo ja pago (US-08 c3).
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
