using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Authentication.Login;

public sealed record LoginQuery(string Email, string Password) : IQuery<LoginResponse>;

public sealed record LoginResponse(Guid UserId, string AccessToken, DateTimeOffset ExpiresAtUtc);
