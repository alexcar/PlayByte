using Microsoft.EntityFrameworkCore;
using PlayByte.Domain.Billing;
using PlayByte.Domain.Billing.Enumerations;
using PlayByte.Domain.Users;

namespace PlayByte.Infrastructure.Persistence.Repositories;

internal sealed class SubscriptionRepository(ApplicationDbContext context) : ISubscriptionRepository
{
    public async Task<Subscription?> GetByIdAsync(SubscriptionId id, CancellationToken ct = default)
        => await context.Subscriptions.FirstOrDefaultAsync(s => s.Id == id, ct);

    /// <summary>Retorna a assinatura "corrente" do usuario (a mais recente nao expirada).</summary>
    public async Task<Subscription?> GetActiveByUserIdAsync(UserId userId, CancellationToken ct = default)
        => await context.Subscriptions
            .Where(s => s.UserId == userId && s.Status != SubscriptionStatus.Expired)
            .OrderByDescending(s => s.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

    public void Add(Subscription subscription) => context.Subscriptions.Add(subscription);
}
