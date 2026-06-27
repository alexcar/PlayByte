using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Catalog.Commands.AddTrack;

public sealed record AddTrackCommand(Guid BandId, Guid AlbumId, string Title, int DurationSeconds) : ICommand<Guid>;
