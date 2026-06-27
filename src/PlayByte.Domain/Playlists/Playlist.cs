using PlayByte.Domain.Abstractions;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Common;
using PlayByte.Domain.Users;

namespace PlayByte.Domain.Playlists;

public readonly record struct PlaylistId(Guid Value)
{
    public static PlaylistId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}

public readonly record struct PlaylistItemId(Guid Value)
{
    public static PlaylistItemId New() => new(Guid.CreateVersion7());
}

public sealed class PlaylistItem : Entity<PlaylistItemId>
{
    private PlaylistItem() { } // EF Core

    internal PlaylistItem(PlaylistId playlistId, TrackId trackId, int position, DateTimeOffset addedAtUtc)
    {
        Id = PlaylistItemId.New();
        PlaylistId = playlistId;
        TrackId = trackId;
        Position = position;
        AddedAtUtc = addedAtUtc;
    }

    public PlaylistId PlaylistId { get; private set; }
    public TrackId TrackId { get; private set; }
    public int Position { get; private set; }
    public DateTimeOffset AddedAtUtc { get; private set; }

    internal void SetPosition(int position) => Position = position;
}

public sealed class Playlist : AggregateRoot<PlaylistId>, IAuditable
{
    private readonly List<PlaylistItem> _items = [];

    private Playlist() { } // EF Core

    private Playlist(PlaylistId id, UserId userId, string name) : base(id)
    {
        UserId = userId;
        Name = name;
    }

    public UserId UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public IReadOnlyList<PlaylistItem> Items => _items.AsReadOnly();
    public int TrackCount => _items.Count;

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static Result<Playlist> Create(UserId userId, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return PlaylistErrors.NameRequired;

        return new Playlist(PlaylistId.New(), userId, name.Trim());
    }

    /// <summary>Adiciona uma faixa; rejeita duplicatas (US-13 c3).</summary>
    public Result AddTrack(TrackId trackId, DateTimeOffset addedAtUtc)
    {
        if (_items.Any(i => i.TrackId == trackId))
            return PlaylistErrors.TrackAlreadyAdded;

        _items.Add(new PlaylistItem(Id, trackId, _items.Count + 1, addedAtUtc));
        return Result.Success();
    }

    /// <summary>Remove uma faixa da playlist e reordena as posições (US-13).</summary>
    public Result RemoveTrack(TrackId trackId)
    {
        var item = _items.FirstOrDefault(i => i.TrackId == trackId);
        if (item is null)
            return PlaylistErrors.TrackNotInPlaylist;

        _items.Remove(item);

        // Reordena para manter as posições contíguas (1..n).
        var ordered = _items.OrderBy(i => i.Position).ToList();
        for (var i = 0; i < ordered.Count; i++)
            ordered[i].SetPosition(i + 1);

        return Result.Success();
    }

    public Result Rename(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return PlaylistErrors.NameRequired;
        Name = name.Trim();
        return Result.Success();
    }
}

public interface IPlaylistRepository
{
    Task<Playlist?> GetByIdAsync(PlaylistId id, CancellationToken ct = default);
    Task<bool> NameExistsForUserAsync(UserId userId, string name, CancellationToken ct = default);
    void Add(Playlist playlist);
}

public static class PlaylistErrors
{
    public static readonly Error NameRequired =
        Error.Validation("Playlist.NameRequired", "O nome da playlist e obrigatorio.");
    public static readonly Error TrackAlreadyAdded =
        Error.Conflict("Playlist.TrackAlreadyAdded", "Esta musica ja esta na playlist.");
    public static readonly Error TrackNotInPlaylist =
        Error.NotFound("Playlist.TrackNotInPlaylist", "Esta musica nao esta na playlist.");
    public static readonly Error NameAlreadyExists =
        Error.Conflict("Playlist.NameAlreadyExists", "Voce ja possui uma playlist com este nome.");
    public static readonly Error NotOwner =
        Error.Forbidden("Playlist.NotOwner", "Esta playlist nao pertence ao usuario.");

    public static Error NotFound(PlaylistId id) =>
        Error.NotFound("Playlist.NotFound", $"Playlist {id} nao encontrada.");
}
