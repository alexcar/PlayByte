using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Favorites.Bands;

public sealed record FavoriteBandCommand(Guid UserId, Guid BandId) : ICommand;
public sealed record UnfavoriteBandCommand(Guid UserId, Guid BandId) : ICommand;
