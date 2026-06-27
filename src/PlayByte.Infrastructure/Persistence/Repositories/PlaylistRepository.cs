using Microsoft.EntityFrameworkCore;
using PlayByte.Domain.Playlists;
using PlayByte.Domain.Users;

namespace PlayByte.Infrastructure.Persistence.Repositories;

internal sealed class PlaylistRepository(ApplicationDbContext context) : IPlaylistRepository
{
    public async Task<Playlist?> GetByIdAsync(PlaylistId id, CancellationToken ct = default)
        => await context.Playlists
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<bool> NameExistsForUserAsync(UserId userId, string name, CancellationToken ct = default)
    {
        var normalized = name.Trim();
        return await context.Playlists
            .AnyAsync(p => p.UserId == userId && p.Name == normalized, ct);
    }

    public void Add(Playlist playlist) => context.Playlists.Add(playlist);
}
