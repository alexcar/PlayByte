using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Common;
using PlayByte.Domain.Favorites;
using PlayByte.Domain.Users;

namespace PlayByte.Application.Favorites.Tracks;

internal sealed class FavoriteTrackCommandHandler(
    IFavoriteTrackRepository favorites, IApplicationDbContext dbContext)
    : ICommandHandler<FavoriteTrackCommand>
{
    public async Task<Result> Handle(FavoriteTrackCommand command, CancellationToken cancellationToken)
    {
        var userId = new UserId(command.UserId);
        var trackId = new TrackId(command.TrackId);

        // Idempotente: se ja for favorito, nada a fazer (US-14 c1).
        var existing = await favorites.GetAsync(userId, trackId, cancellationToken);
        if (existing is not null)
            return Result.Success();

        favorites.Add(FavoriteTrack.Create(userId, trackId));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class UnfavoriteTrackCommandHandler(
    IFavoriteTrackRepository favorites, IApplicationDbContext dbContext)
    : ICommandHandler<UnfavoriteTrackCommand>
{
    public async Task<Result> Handle(UnfavoriteTrackCommand command, CancellationToken cancellationToken)
    {
        var existing = await favorites.GetAsync(
            new UserId(command.UserId), new TrackId(command.TrackId), cancellationToken);

        if (existing is null)
            return Result.Success(); // idempotente (US-14 c2)

        favorites.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
