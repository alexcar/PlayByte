using Microsoft.EntityFrameworkCore;
using PlayByte.Domain.Users;
using PlayByte.Domain.Users.ValueObjects;

namespace PlayByte.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default)
        => await context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default)
        => await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<bool> EmailExistsAsync(Email email, CancellationToken ct = default)
        => await context.Users.AnyAsync(u => u.Email == email, ct);

    public void Add(User user) => context.Users.Add(user);

    public void Remove(User user) => context.Users.Remove(user); // -> soft delete via interceptor
}
