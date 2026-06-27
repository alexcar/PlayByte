namespace PlayByte.Domain.Users;

/// <summary>Id fortemente tipado. UUID v7 (sequencial, amigavel a indices clusterizados).</summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}
