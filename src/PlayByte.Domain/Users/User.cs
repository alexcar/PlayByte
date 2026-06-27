using PlayByte.Domain.Abstractions;
using PlayByte.Domain.Common;
using PlayByte.Domain.Users.Events;
using PlayByte.Domain.Users.ValueObjects;

namespace PlayByte.Domain.Users;

public sealed class User : AggregateRoot<UserId>, IAuditable, ISoftDeletable
{
    private User() { } // EF Core

    private User(UserId id, UserName name, Email email, PasswordHash passwordHash) : base(id)
    {
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        IsActive = false;
    }

    public UserName Name { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public PasswordHash PasswordHash { get; private set; } = default!;
    public bool IsActive { get; private set; }

    // IAuditable (atribuido pelo AuditingInterceptor)
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    // ISoftDeletable
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public void MarkAsDeleted(DateTimeOffset whenUtc)
    {
        if (IsDeleted) return; // idempotente
        IsDeleted = true;
        DeletedAtUtc = whenUtc;
    }

    /// <summary>
    /// Factory. NAO valida unicidade de e-mail: isso e regra set-level,
    /// resolvida na Application (consulta) + indice unico no banco (garantia).
    /// </summary>
    public static Result<User> Register(UserName name, Email email, PasswordHash passwordHash)
    {
        var user = new User(UserId.New(), name, email, passwordHash);
        user.RaiseDomainEvent(new UserRegistered(user.Id, email.Value));
        return user;
    }

    public Result Activate()
    {
        if (IsActive) return UserErrors.AlreadyActive;
        IsActive = true;
        return Result.Success();
    }

    public Result RequestPasswordReset(string token)
    {
        RaiseDomainEvent(new PasswordResetRequested(Id, Email.Value, token));
        return Result.Success();
    }

    public void ChangePasswordHash(PasswordHash newHash) => PasswordHash = newHash;

    /// <summary>Atualiza nome e e-mail do perfil (US-Perfil). Unicidade de e-mail
    /// e regra set-level (verificada na Application + indice unico no banco).</summary>
    public void ChangeProfile(UserName name, Email email)
    {
        Name = name;
        Email = email;
    }
}
