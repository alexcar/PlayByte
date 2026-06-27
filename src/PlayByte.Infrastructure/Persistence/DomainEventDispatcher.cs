using Microsoft.Extensions.DependencyInjection;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Abstractions;

namespace PlayByte.Infrastructure.Persistence;

/// <summary>
/// Despacha eventos de dominio em processo: para cada evento, resolve os
/// IDomainEventHandler&lt;TEvento&gt; registrados no container e os invoca.
/// </summary>
internal sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle))!;

            foreach (var handler in serviceProvider.GetServices(handlerType))
            {
                if (handler is null)
                    continue;

                await (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
            }
        }
    }
}
