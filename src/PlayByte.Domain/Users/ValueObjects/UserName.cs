using PlayByte.Domain.Common;

namespace PlayByte.Domain.Users.ValueObjects;

public sealed record UserName
{
    public const int MinLength = 3;
    public const int MaxLength = 50;

    public string Value { get; }

    private UserName(string value) => Value = value;

    public static Result<UserName> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return UserNameErrors.Empty;

        var normalized = string.Join(' ', input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        return normalized.Length switch
        {
            < MinLength => UserNameErrors.TooShort,
            > MaxLength => UserNameErrors.TooLong,
            _ => new UserName(normalized)
        };
    }

    public override string ToString() => Value;
}

public static class UserNameErrors
{
    public static readonly Error Empty =
        Error.Validation("UserName.Empty", "O nome de usuario nao pode ser vazio.");
    public static readonly Error TooShort =
        Error.Validation("UserName.TooShort", $"O nome deve ter ao menos {UserName.MinLength} caracteres.");
    public static readonly Error TooLong =
        Error.Validation("UserName.TooLong", $"O nome deve ter no maximo {UserName.MaxLength} caracteres.");
}
