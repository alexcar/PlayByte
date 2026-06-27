using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PlayByte.Domain.Abstractions;

namespace PlayByte.Infrastructure.Persistence.Interceptors;

/// <summary>Converte Remove() de entidades ISoftDeletable em UPDATE (soft delete).</summary>
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        foreach (EntityEntry<ISoftDeletable> entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State is not EntityState.Deleted)
                continue;

            entry.State = EntityState.Modified;     // nao apaga a linha
            entry.Entity.MarkAsDeleted(now);         // seta IsDeleted/DeletedAtUtc
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
