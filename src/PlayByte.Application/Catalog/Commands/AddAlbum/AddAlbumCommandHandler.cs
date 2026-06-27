using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Catalog.Commands.AddAlbum;

internal sealed class AddAlbumCommandHandler(IBandRepository bands, IApplicationDbContext dbContext)
    : ICommandHandler<AddAlbumCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddAlbumCommand command, CancellationToken cancellationToken)
    {
        var band = await bands.GetByIdAsync(new BandId(command.BandId), cancellationToken);
        if (band is null) return CatalogErrors.BandNotFound(new BandId(command.BandId));

        var result = band.AddAlbum(command.Title, command.ReleaseYear);
        if (result.IsFailure) return result.Error;

        await dbContext.SaveChangesAsync(cancellationToken);
        return result.Value.Id.Value;
    }
}
