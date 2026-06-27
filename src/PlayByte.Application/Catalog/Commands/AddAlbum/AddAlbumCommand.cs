using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Catalog.Commands.AddAlbum;

public sealed record AddAlbumCommand(Guid BandId, string Title, int ReleaseYear) : ICommand<Guid>;
