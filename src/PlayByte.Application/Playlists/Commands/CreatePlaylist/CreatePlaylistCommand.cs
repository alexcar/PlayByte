using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Playlists.Commands.CreatePlaylist;

public sealed record CreatePlaylistCommand(Guid UserId, string Name) : ICommand<Guid>;
