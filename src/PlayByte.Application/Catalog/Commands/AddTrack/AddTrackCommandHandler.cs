using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Catalog.Commands.AddTrack;

internal sealed class AddTrackCommandHandler(IBandRepository bands, IApplicationDbContext dbContext)
    : ICommandHandler<AddTrackCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddTrackCommand command, CancellationToken cancellationToken)
    {
        var band = await bands.GetByIdAsync(new BandId(command.BandId), cancellationToken);
        if (band is null) return CatalogErrors.BandNotFound(new BandId(command.BandId));

        var result = band.AddTrack(new AlbumId(command.AlbumId), command.Title, command.DurationSeconds);
        if (result.IsFailure) return result.Error;

        await dbContext.SaveChangesAsync(cancellationToken);
        return result.Value.Id.Value;
    }
}
