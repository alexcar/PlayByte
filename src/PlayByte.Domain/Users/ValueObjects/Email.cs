using System.Text.RegularExpressions;
using PlayByte.Domain.Common;

namespace PlayByte.Domain.Users.ValueObjects;

public sealed partial record Email
{
    public const int MaxLength = 254; // RFC 5321

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return EmailErrors.Empty;

        var normalized = input.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            return EmailErrors.TooLong;

        return FormatRegex().IsMatch(normalized)
            ? new Email(normalized)
            : EmailErrors.InvalidFormat;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex FormatRegex();
}

public static class EmailErrors
{
    public static readonly Error Empty =
        Error.Validation("Email.Empty", "O e-mail nao pode ser vazio.");
    public static readonly Error TooLong =
        Error.Validation("Email.TooLong", "O e-mail excede o tamanho maximo permitido.");
    public static readonly Error InvalidFormat =
        Error.Validation("Email.InvalidFormat", "O formato do e-mail e invalido.");
}
