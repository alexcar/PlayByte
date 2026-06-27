using PlayByte.Domain.Abstractions;
using PlayByte.Domain.Common;

namespace PlayByte.Domain.Users;

public readonly record struct PasswordResetTokenId(Guid Value)
{
    public static PasswordResetTokenId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}

public sealed class PasswordResetToken : AggregateRoot<PasswordResetTokenId>, IAuditable
{
    private PasswordResetToken() { } // EF Core

    private PasswordResetToken(PasswordResetTokenId id, UserId userId, string tokenHash, DateTimeOffset expiresAtUtc)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public UserId UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty; // guardamos o HASH, nunca o token cru
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset? UsedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    /// <summary>Emite um token valido por 30 minutos.</summary>
    public static PasswordResetToken Issue(UserId userId, string tokenHash, DateTimeOffset nowUtc) =>
        new(PasswordResetTokenId.New(), userId, tokenHash, nowUtc.AddMinutes(30));

    public bool IsValid(DateTimeOffset nowUtc) => UsedAtUtc is null && nowUtc < ExpiresAtUtc;

    public Result MarkUsed(DateTimeOffset nowUtc)
    {
        if (!IsValid(nowUtc))
            return PasswordResetTokenErrors.InvalidOrExpired;
        UsedAtUtc = nowUtc;
        return Result.Success();
    }
}

public interface IPasswordResetTokenRepository
{
    void Add(PasswordResetToken token);
    Task<PasswordResetToken?> GetActiveByHashAsync(string tokenHash, CancellationToken ct = default);
}

public static class PasswordResetTokenErrors
{
    public static readonly Error InvalidOrExpired =
        Error.Validation("PasswordReset.InvalidOrExpired", "O link de recuperacao e invalido ou expirou.");
}
