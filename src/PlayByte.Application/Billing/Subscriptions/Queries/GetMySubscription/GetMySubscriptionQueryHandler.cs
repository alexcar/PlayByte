using Dapper;
using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Billing;
using PlayByte.Domain.Billing.Enumerations;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Billing.Subscriptions.Queries.GetMySubscription;

internal sealed class GetMySubscriptionQueryHandler(ISqlConnectionFactory connectionFactory, TimeProvider timeProvider)
    : IQueryHandler<GetMySubscriptionQuery, MySubscriptionResponse>
{
    private sealed record Row(Guid Id, string PlanName, int Status, DateOnly CurrentPeriodEnd);

    public async Task<Result<MySubscriptionResponse>> Handle(
        GetMySubscriptionQuery query, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT TOP (1)
                    id              AS Id,
                    plan_name       AS PlanName,
                    status          AS Status,
                    current_period_end AS CurrentPeriodEnd
            FROM    subscriptions
            WHERE   user_id = @UserId
              AND   status <> 6           -- != Expired
            ORDER BY created_at_utc DESC
            """;

        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<Row>(
            new CommandDefinition(sql, new { query.UserId }, cancellationToken: cancellationToken));

        if (row is null)
            return SubscriptionErrors.NoActiveSubscription;

        var status = SubscriptionStatus.FromId(row.Status);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var hasAccess = status.GrantsAccess && today < row.CurrentPeriodEnd;

        return new MySubscriptionResponse(row.Id, row.PlanName, status.Name, row.CurrentPeriodEnd, hasAccess);
    }
}
