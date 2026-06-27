using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Common;
using PlayByte.Domain.Favorites;
using PlayByte.Domain.Users;

namespace PlayByte.Application.Favorites.Bands;

internal sealed class FavoriteBandCommandHandler(
    IFavoriteBandRepository favorites, IApplicationDbContext dbContext)
    : ICommandHandler<FavoriteBandCommand>
{
    public async Task<Result> Handle(FavoriteBandCommand command, CancellationToken cancellationToken)
    {
        var userId = new UserId(command.UserId);
        var bandId = new BandId(command.BandId);

        var existing = await favorites.GetAsync(userId, bandId, cancellationToken);
        if (existing is not null)
            return Result.Success();

        favorites.Add(FavoriteBand.Create(userId, bandId));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class UnfavoriteBandCommandHandler(
    IFavoriteBandRepository favorites, IApplicationDbContext dbContext)
    : ICommandHandler<UnfavoriteBandCommand>
{
    public async Task<Result> Handle(UnfavoriteBandCommand command, CancellationToken cancellationToken)
    {
        var existing = await favorites.GetAsync(
            new UserId(command.UserId), new BandId(command.BandId), cancellationToken);

        if (existing is null)
            return Result.Success();

        favorites.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
