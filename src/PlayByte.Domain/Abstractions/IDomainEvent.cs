namespace PlayByte.Domain.Abstractions;

/// <summary>
/// Marca um evento de dominio. Interface propria do Domain, sem dependencias externas
/// (a camada de Domain nao conhece MediatR nem qualquer detalhe de infraestrutura).
/// O despacho dos eventos e' responsabilidade da Application/Infrastructure.
/// </summary>
public interface IDomainEvent;
