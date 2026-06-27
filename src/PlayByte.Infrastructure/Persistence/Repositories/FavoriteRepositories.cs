using Microsoft.EntityFrameworkCore;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Favorites;
using PlayByte.Domain.Users;

namespace PlayByte.Infrastructure.Persistence.Repositories;

internal sealed class FavoriteTrackRepository(ApplicationDbContext context) : IFavoriteTrackRepository
{
    public async Task<FavoriteTrack?> GetAsync(UserId userId, TrackId trackId, CancellationToken ct = default)
        => await context.FavoriteTracks
            .FirstOrDefaultAsync(f => f.UserId == userId && f.TrackId == trackId, ct);

    public void Add(FavoriteTrack favorite) => context.FavoriteTracks.Add(favorite);
    public void Remove(FavoriteTrack favorite) => context.FavoriteTracks.Remove(favorite);
}

internal sealed class FavoriteBandRepository(ApplicationDbContext context) : IFavoriteBandRepository
{
    public async Task<FavoriteBand?> GetAsync(UserId userId, BandId bandId, CancellationToken ct = default)
        => await context.FavoriteBands
            .FirstOrDefaultAsync(f => f.UserId == userId && f.BandId == bandId, ct);

    public void Add(FavoriteBand favorite) => context.FavoriteBands.Add(favorite);
    public void Remove(FavoriteBand favorite) => context.FavoriteBands.Remove(favorite);
}
