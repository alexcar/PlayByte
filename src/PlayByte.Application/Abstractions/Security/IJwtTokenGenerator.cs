using PlayByte.Domain.Users;

namespace PlayByte.Application.Abstractions.Security;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAtUtc);

public interface IJwtTokenGenerator
{
    AccessToken Generate(UserId userId, string email);
}
