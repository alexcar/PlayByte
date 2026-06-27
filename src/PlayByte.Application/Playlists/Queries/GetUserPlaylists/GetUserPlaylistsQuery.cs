using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Playlists.Queries.GetUserPlaylists;

public sealed record GetUserPlaylistsQuery(Guid UserId) : IQuery<IReadOnlyList<PlaylistSummary>>;

public sealed record PlaylistSummary(Guid Id, string Name, int TrackCount);
