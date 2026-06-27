using PlayByte.Domain.Abstractions;

namespace PlayByte.Domain.Catalog;

public sealed class Track : Entity<TrackId>
{
    private Track() { } // EF Core

    private Track(TrackId id, AlbumId albumId, string title, int durationSeconds)
    {
        Id = id;
        AlbumId = albumId;
        Title = title;
        DurationSeconds = durationSeconds;
    }

    public AlbumId AlbumId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public int DurationSeconds { get; private set; }

    internal static Track Create(AlbumId albumId, string title, int durationSeconds) =>
        new(TrackId.New(), albumId, title.Trim(), durationSeconds);
}
