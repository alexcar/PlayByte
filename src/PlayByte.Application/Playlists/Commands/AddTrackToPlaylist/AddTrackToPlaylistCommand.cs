using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Playlists.Commands.AddTrackToPlaylist;

public sealed record AddTrackToPlaylistCommand(Guid UserId, Guid PlaylistId, Guid TrackId) : ICommand<int>;
