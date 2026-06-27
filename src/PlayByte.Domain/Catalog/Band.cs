using PlayByte.Domain.Abstractions;
using PlayByte.Domain.Catalog.Events;
using PlayByte.Domain.Common;

namespace PlayByte.Domain.Catalog;

public sealed class Band : AggregateRoot<BandId>, IAuditable
{
    private readonly List<Album> _albums = [];

    private Band() { } // EF Core

    private Band(BandId id, string name, string? coverImageUrl) : base(id)
    {
        Name = name;
        CoverImageUrl = coverImageUrl;
    }

    public string Name { get; private set; } = string.Empty;
    public string? CoverImageUrl { get; private set; }
    public IReadOnlyList<Album> Albums => _albums.AsReadOnly();

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static Result<Band> Create(string? name, string? coverImageUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            return CatalogErrors.BandNameRequired;

        var band = new Band(BandId.New(), name.Trim(), coverImageUrl?.Trim());
        band.RaiseDomainEvent(new BandCreated(band.Id, band.Name));
        return band;
    }

    public Result<Album> AddAlbum(string? title, int releaseYear)
    {
        if (string.IsNullOrWhiteSpace(title))
            return CatalogErrors.AlbumTitleRequired;
        if (releaseYear < 1900 || releaseYear > 2200)
            return CatalogErrors.InvalidReleaseYear;

        var album = Album.Create(Id, title, releaseYear);
        _albums.Add(album);
        return album;
    }

    public Result<Track> AddTrack(AlbumId albumId, string? title, int durationSeconds)
    {
        var album = _albums.FirstOrDefault(a => a.Id == albumId);
        if (album is null)
            return CatalogErrors.AlbumNotFound;
        if (string.IsNullOrWhiteSpace(title))
            return CatalogErrors.TrackTitleRequired;

        return album.AddTrack(title, durationSeconds);
    }
}
