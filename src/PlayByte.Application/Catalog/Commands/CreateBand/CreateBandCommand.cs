using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Catalog.Commands.CreateBand;

public sealed record CreateBandCommand(string Name, string? CoverImageUrl) : ICommand<Guid>;
