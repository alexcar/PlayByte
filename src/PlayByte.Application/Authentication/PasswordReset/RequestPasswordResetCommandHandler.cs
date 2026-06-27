using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Common;
using PlayByte.Domain.Users;
using PlayByte.Domain.Users.ValueObjects;

namespace PlayByte.Application.Authentication.PasswordReset;

/// <summary>
/// Sempre retorna sucesso, exista o e-mail ou nao (US-03 c3: nao enumera contas).
/// Se existir, emite um token, guarda o HASH e levanta PasswordResetRequested
/// (o token cru segue por e-mail via handler de evento).
/// </summary>
internal sealed class RequestPasswordResetCommandHandler(
    IUserRepository users,
    IPasswordResetTokenRepository tokens,
    IApplicationDbContext dbContext,
    TimeProvider timeProvider)
    : ICommandHandler<RequestPasswordResetCommand>
{
    public async Task<Result> Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result.Success(); // mesma resposta

        var user = await users.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null)
            return Result.Success(); // mesma resposta

        var rawToken = ResetTokenHasher.NewToken();
        var token = PasswordResetToken.Issue(user.Id, ResetTokenHasher.Hash(rawToken), timeProvider.GetUtcNow());
        tokens.Add(token);

        // Levanta o evento (carrega o token CRU para montar o link no e-mail).
        user.RequestPasswordReset(rawToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
