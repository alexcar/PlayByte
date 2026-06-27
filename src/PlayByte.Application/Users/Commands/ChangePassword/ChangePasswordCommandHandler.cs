using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Application.Abstractions.Security;
using PlayByte.Domain.Common;
using PlayByte.Domain.Users;
using PlayByte.Domain.Users.ValueObjects;

namespace PlayByte.Application.Users.Commands.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IApplicationDbContext dbContext)
    : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(new UserId(command.UserId), cancellationToken);
        if (user is null)
            return UserErrors.NotFound(new UserId(command.UserId));

        // A senha atual precisa conferir (impede trocar com qualquer senha).
        if (!passwordHasher.Verify(command.CurrentPassword, user.PasswordHash.Value))
            return UserErrors.IncorrectCurrentPassword;

        var hashResult = PasswordHash.Create(passwordHasher.Hash(command.NewPassword));
        if (hashResult.IsFailure)
            return hashResult.Error;

        user.ChangePasswordHash(hashResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
