namespace PlayByte.Application.Abstractions.Security;

/// <summary>Porta para hashing. Implementacao concreta (BCrypt) vive na Infrastructure.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
