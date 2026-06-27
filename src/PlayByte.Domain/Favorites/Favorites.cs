using PlayByte.Domain.Abstractions;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Users;

namespace PlayByte.Domain.Favorites;

public readonly record struct FavoriteTrackId(Guid Value)
{
    public static FavoriteTrackId New() => new(Guid.CreateVersion7());
}

public readonly record struct FavoriteBandId(Guid Value)
{
    public static FavoriteBandId New() => new(Guid.CreateVersion7());
}

/// <summary>Favorito de musica (US-14). Unicidade por (UserId, TrackId).</summary>
public sealed class FavoriteTrack : AggregateRoot<FavoriteTrackId>, IAuditable
{
    private FavoriteTrack() { } // EF Core

    private FavoriteTrack(FavoriteTrackId id, UserId userId, TrackId trackId) : base(id)
    {
        UserId = userId;
        TrackId = trackId;
    }

    public UserId UserId { get; private set; }
    public TrackId TrackId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static FavoriteTrack Create(UserId userId, TrackId trackId) =>
        new(FavoriteTrackId.New(), userId, trackId);
}

/// <summary>Favorito de banda (US-15). Unicidade por (UserId, BandId).</summary>
public sealed class FavoriteBand : AggregateRoot<FavoriteBandId>, IAuditable
{
    private FavoriteBand() { } // EF Core

    private FavoriteBand(FavoriteBandId id, UserId userId, BandId bandId) : base(id)
    {
        UserId = userId;
        BandId = bandId;
    }

    public UserId UserId { get; private set; }
    public BandId BandId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static FavoriteBand Create(UserId userId, BandId bandId) =>
        new(FavoriteBandId.New(), userId, bandId);
}

public interface IFavoriteTrackRepository
{
    Task<FavoriteTrack?> GetAsync(UserId userId, TrackId trackId, CancellationToken ct = default);
    void Add(FavoriteTrack favorite);
    void Remove(FavoriteTrack favorite);
}

public interface IFavoriteBandRepository
{
    Task<FavoriteBand?> GetAsync(UserId userId, BandId bandId, CancellationToken ct = default);
    void Add(FavoriteBand favorite);
    void Remove(FavoriteBand favorite);
}
