using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Application.Abstractions.Security;
using PlayByte.Domain.Common;
using PlayByte.Domain.Users;
using PlayByte.Domain.Users.ValueObjects;

namespace PlayByte.Application.Users.Commands.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IApplicationDbContext dbContext)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        // 1. Value objects (validacao de forma do dominio)
        var nameResult = UserName.Create(command.Name);
        if (nameResult.IsFailure) return nameResult.Error;

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure) return emailResult.Error;

        // 2. Regra set-level: unicidade (consulta). O indice unico no banco e a garantia real.
        if (await users.EmailExistsAsync(emailResult.Value, cancellationToken))
            return UserErrors.EmailAlreadyInUse;

        // 3. Hashing e' Infrastructure, atras da porta:
        var hash = passwordHasher.Hash(command.Password);
        var hashResult = PasswordHash.Create(hash);
        if (hashResult.IsFailure) return hashResult.Error;

        // 4. Factory de dominio (so cuida das invariantes do agregado):
        var userResult = User.Register(nameResult.Value, emailResult.Value, hashResult.Value);
        if (userResult.IsFailure) return userResult.Error;

        users.Add(userResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        return userResult.Value.Id.Value;
    }
}
