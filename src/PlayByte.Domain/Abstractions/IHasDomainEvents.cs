namespace PlayByte.Domain.Abstractions;

/// <summary>
/// Exposto pela raiz de agregado para que a Infrastructure colete/despache
/// eventos sem precisar conhecer o parametro de tipo TId.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
