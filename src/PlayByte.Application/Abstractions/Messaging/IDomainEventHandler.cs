using PlayByte.Domain.Abstractions;

namespace PlayByte.Application.Abstractions.Messaging;

/// <summary>
/// Handler de um evento de dominio. Substitui o INotificationHandler do MediatR para
/// que a camada de Domain nao precise herdar de tipos externos (Clean Architecture).
/// </summary>
public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken);
}
