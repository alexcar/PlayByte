namespace PlayByte.Domain.Abstractions;

/// <summary>
/// Soft delete. O SoftDeleteInterceptor converte Remove() em UPDATE,
/// e um global query filter no DbContext esconde os registros marcados.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedAtUtc { get; }
    void MarkAsDeleted(DateTimeOffset whenUtc);
}
