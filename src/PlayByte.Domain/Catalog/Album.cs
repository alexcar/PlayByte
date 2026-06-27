using PlayByte.Domain.Abstractions;
using PlayByte.Domain.Common;

namespace PlayByte.Domain.Catalog;

public sealed class Album : Entity<AlbumId>
{
    private readonly List<Track> _tracks = [];

    private Album() { } // EF Core

    private Album(AlbumId id, BandId bandId, string title, int releaseYear)
    {
        Id = id;
        BandId = bandId;
        Title = title;
        ReleaseYear = releaseYear;
    }

    public BandId BandId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public int ReleaseYear { get; private set; }
    public IReadOnlyList<Track> Tracks => _tracks.AsReadOnly();

    internal static Album Create(BandId bandId, string title, int releaseYear) =>
        new(AlbumId.New(), bandId, title.Trim(), releaseYear);

    internal Result<Track> AddTrack(string title, int durationSeconds)
    {
        if (string.IsNullOrWhiteSpace(title))
            return CatalogErrors.TrackTitleRequired;
        if (durationSeconds <= 0)
            return CatalogErrors.InvalidDuration;

        var track = Track.Create(Id, title, durationSeconds);
        _tracks.Add(track);
        return track;
    }
}
