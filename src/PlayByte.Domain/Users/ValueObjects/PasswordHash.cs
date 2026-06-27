using PlayByte.Domain.Common;

namespace PlayByte.Domain.Users.ValueObjects;

/// <summary>
/// Representa um hash JA computado pela Infrastructure (IPasswordHasher).
/// O dominio nunca conhece o algoritmo nem a senha em texto plano.
/// </summary>
public sealed record PasswordHash
{
    public string Value { get; }

    private PasswordHash(string value) => Value = value;

    public static Result<PasswordHash> Create(string? hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return PasswordHashErrors.Empty;

        return new PasswordHash(hash);
    }

    // Evita vazamento acidental em logs/exceptions.
    public override string ToString() => "********";
}

public static class PasswordHashErrors
{
    public static readonly Error Empty =
        Error.Validation("PasswordHash.Empty", "O hash da senha nao pode ser vazio.");
}
