using System.Security.Cryptography;
using System.Text;

namespace PlayByte.Application.Authentication.PasswordReset;

/// <summary>
/// Tokens de reset sao segredos de alta entropia; guardamos apenas o hash SHA-256
/// (hex) no banco para que um vazamento da base nao permita redefinir senhas.
/// </summary>
internal static class ResetTokenHasher
{
    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    public static string NewToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
