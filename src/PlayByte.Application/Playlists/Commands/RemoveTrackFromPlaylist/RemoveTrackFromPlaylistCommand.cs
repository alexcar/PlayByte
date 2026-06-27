using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Playlists.Commands.RemoveTrackFromPlaylist;

/// <summary>Remove uma faixa da playlist e retorna a quantidade total restante.</summary>
public sealed record RemoveTrackFromPlaylistCommand(Guid UserId, Guid PlaylistId, Guid TrackId) : ICommand<int>;
