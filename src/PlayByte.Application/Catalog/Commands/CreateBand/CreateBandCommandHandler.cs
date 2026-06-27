using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Catalog.Commands.CreateBand;

internal sealed class CreateBandCommandHandler(IBandRepository bands, IApplicationDbContext dbContext)
    : ICommandHandler<CreateBandCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateBandCommand command, CancellationToken cancellationToken)
    {
        var result = Band.Create(command.Name, command.CoverImageUrl);
        if (result.IsFailure) return result.Error;

        bands.Add(result.Value);
        await dbContext.SaveChangesAsync(cancellationToken);
        return result.Value.Id.Value;
    }
}
