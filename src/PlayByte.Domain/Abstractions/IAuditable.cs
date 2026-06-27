namespace PlayByte.Domain.Abstractions;

/// <summary>Auditoria de criacao/alteracao preenchida pelo AuditingInterceptor.</summary>
public interface IAuditable
{
    DateTimeOffset CreatedAtUtc { get; }
    DateTimeOffset? UpdatedAtUtc { get; }
}
