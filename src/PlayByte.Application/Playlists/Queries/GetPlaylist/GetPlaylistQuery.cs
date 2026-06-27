using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Playlists.Queries.GetPlaylist;

public sealed record GetPlaylistQuery(Guid UserId, Guid PlaylistId) : IQuery<PlaylistDetailsResponse>;

public sealed record PlaylistDetailsResponse(Guid Id, string Name, int TrackCount, IReadOnlyList<PlaylistTrack> Tracks);
public sealed record PlaylistTrack(Guid TrackId, string Title, string BandName, int Position);
