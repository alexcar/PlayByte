using PlayByte.Domain.Users.ValueObjects;

namespace PlayByte.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(Email email, CancellationToken ct = default);
    void Add(User user);
    void Remove(User user);
}
