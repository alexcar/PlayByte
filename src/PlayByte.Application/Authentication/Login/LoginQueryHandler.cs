using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Application.Abstractions.Security;
using PlayByte.Domain.Common;
using PlayByte.Domain.Users;
using PlayByte.Domain.Users.ValueObjects;

namespace PlayByte.Application.Authentication.Login;

internal sealed class LoginQueryHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator)
    : IQueryHandler<LoginQuery, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginQuery query, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(query.Email);
        if (emailResult.IsFailure)
            return UserErrors.InvalidCredentials; // nao revela o motivo (US-02 c2)

        var user = await users.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null || !passwordHasher.Verify(query.Password, user.PasswordHash.Value))
            return UserErrors.InvalidCredentials;

        var token = tokenGenerator.Generate(user.Id, user.Email.Value);

        return new LoginResponse(user.Id.Value, token.Token, token.ExpiresAtUtc);
    }
}
