using PlayByte.Domain.Abstractions;

namespace PlayByte.Domain.Users.Events;

public sealed record PasswordResetRequested(UserId UserId, string Email, string Token) : IDomainEvent;
