using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Application.Abstractions.Security;
using PlayByte.Domain.Common;
using PlayByte.Domain.Users;
using PlayByte.Domain.Users.ValueObjects;

namespace PlayByte.Application.Authentication.PasswordReset;

internal sealed class ResetPasswordCommandHandler(
    IPasswordResetTokenRepository tokens,
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IApplicationDbContext dbContext,
    TimeProvider timeProvider)
    : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var token = await tokens.GetActiveByHashAsync(ResetTokenHasher.Hash(command.Token), cancellationToken);
        if (token is null || !token.IsValid(now))
            return PasswordResetTokenErrors.InvalidOrExpired;

        var user = await users.GetByIdAsync(token.UserId, cancellationToken);
        if (user is null)
            return PasswordResetTokenErrors.InvalidOrExpired;

        var hashResult = PasswordHash.Create(passwordHasher.Hash(command.NewPassword));
        if (hashResult.IsFailure)
            return hashResult.Error;

        user.ChangePasswordHash(hashResult.Value);
        token.MarkUsed(now); // invalida o link (US-03 c5)

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
