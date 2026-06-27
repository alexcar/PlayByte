using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Favorites.Tracks;

public sealed record FavoriteTrackCommand(Guid UserId, Guid TrackId) : ICommand;
public sealed record UnfavoriteTrackCommand(Guid UserId, Guid TrackId) : ICommand;
