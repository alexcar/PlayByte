using PlayByte.Domain.Abstractions;

namespace PlayByte.Application.Abstractions.Messaging;

/// <summary>
/// Despacha eventos de dominio (em processo) para os respectivos IDomainEventHandler.
/// A implementacao vive na Infrastructure; o despacho ocorre apos o commit do DbContext.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
