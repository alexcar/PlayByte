using PlayByte.Domain.Common;

namespace PlayByte.Domain.Users;

public static class UserErrors
{
    public static readonly Error EmailAlreadyInUse =
        Error.Conflict("User.EmailAlreadyInUse", "Ja existe um usuario com este e-mail.");

    public static readonly Error AlreadyActive =
        Error.Conflict("User.AlreadyActive", "O usuario ja esta ativo.");


    public static readonly Error InvalidCredentials =
        Error.Unauthorized("User.InvalidCredentials", "E-mail ou senha invalidos.");

    // Validation (400) de proposito: 401 dispararia o logout automatico no frontend.
    public static readonly Error IncorrectCurrentPassword =
        Error.Validation("User.IncorrectCurrentPassword", "A senha atual esta incorreta.");

    public static Error NotFound(UserId id) =>
        Error.NotFound("User.NotFound", $"Usuario '{id}' nao encontrado.");
}
