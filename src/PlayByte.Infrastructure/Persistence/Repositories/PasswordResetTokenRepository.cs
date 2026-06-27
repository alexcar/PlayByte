using Microsoft.EntityFrameworkCore;
using PlayByte.Domain.Users;

namespace PlayByte.Infrastructure.Persistence.Repositories;

internal sealed class PasswordResetTokenRepository(ApplicationDbContext context)
    : IPasswordResetTokenRepository
{
    public void Add(PasswordResetToken token) => context.PasswordResetTokens.Add(token);

    public async Task<PasswordResetToken?> GetActiveByHashAsync(string tokenHash, CancellationToken ct = default)
        => await context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.UsedAtUtc == null, ct);
}
