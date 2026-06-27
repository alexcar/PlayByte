using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Abstractions;

namespace PlayByte.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Despacha os eventos de dominio APOS o commit do SaveChanges (em processo).
/// O despacho roda num escopo de DI proprio, portanto handlers que persistem usam
/// um DbContext independente - evitando reentrancia no DbContext que disparou o evento.
/// </summary>
public sealed class DispatchDomainEventsInterceptor(IServiceScopeFactory scopeFactory) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            await DispatchAsync(eventData.Context, cancellationToken);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchAsync(DbContext context, CancellationToken cancellationToken)
    {
        var aggregates = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregates.SelectMany(a => a.DomainEvents).ToList();

        // Limpa antes de despachar para nao reprocessar em um SaveChanges subsequente.
        aggregates.ForEach(a => a.ClearDomainEvents());

        if (domainEvents.Count == 0)
            return;

        using var scope = scopeFactory.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();
        await dispatcher.DispatchAsync(domainEvents, cancellationToken);
    }
}
