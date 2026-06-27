using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PlayByte.Domain.Abstractions;

namespace PlayByte.Infrastructure.Persistence.Interceptors;

/// <summary>Preenche CreatedAtUtc/UpdatedAtUtc para entidades IAuditable.</summary>
public sealed class AuditingInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is not null)
            ApplyAudit(context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ApplyAudit(DbContext context)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (EntityEntry<IAuditable> entry in context.ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(IAuditable.CreatedAtUtc)).CurrentValue = now;
                    break;
                case EntityState.Modified:
                    entry.Property(nameof(IAuditable.UpdatedAtUtc)).CurrentValue = now;
                    break;
            }
        }
    }
}
