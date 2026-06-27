using PlayByte.Domain.Abstractions;

namespace PlayByte.Domain.Users.Events;

public sealed record UserRegistered(UserId UserId, string Email) : IDomainEvent;
