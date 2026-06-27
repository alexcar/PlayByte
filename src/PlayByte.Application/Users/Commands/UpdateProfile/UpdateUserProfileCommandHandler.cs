using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Application.Users.Queries.GetUserById;
using PlayByte.Domain.Common;
using PlayByte.Domain.Users;
using PlayByte.Domain.Users.ValueObjects;

namespace PlayByte.Application.Users.Commands.UpdateProfile;

internal sealed class UpdateUserProfileCommandHandler(
    IUserRepository users,
    IApplicationDbContext dbContext)
    : ICommandHandler<UpdateUserProfileCommand, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(UpdateUserProfileCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(new UserId(command.UserId), cancellationToken);
        if (user is null)
            return UserErrors.NotFound(new UserId(command.UserId));

        var nameResult = UserName.Create(command.Name);
        if (nameResult.IsFailure) return nameResult.Error;

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure) return emailResult.Error;

        // Unicidade do e-mail (set-level): so checa se mudou.
        if (emailResult.Value.Value != user.Email.Value
            && await users.EmailExistsAsync(emailResult.Value, cancellationToken))
            return UserErrors.EmailAlreadyInUse;

        user.ChangeProfile(nameResult.Value, emailResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UserResponse(user.Id.Value, user.Name.Value, user.Email.Value, user.IsActive, user.CreatedAtUtc);
    }
}
