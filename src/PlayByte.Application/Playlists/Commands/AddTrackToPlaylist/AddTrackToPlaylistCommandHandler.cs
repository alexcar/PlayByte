using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Common;
using PlayByte.Domain.Playlists;
using PlayByte.Domain.Users;

namespace PlayByte.Application.Playlists.Commands.AddTrackToPlaylist;

/// <summary>Retorna a quantidade total de faixas apos a adicao (US-13 c4).</summary>
internal sealed class AddTrackToPlaylistCommandHandler(
    IPlaylistRepository playlists, IApplicationDbContext dbContext, TimeProvider timeProvider)
    : ICommandHandler<AddTrackToPlaylistCommand, int>
{
    public async Task<Result<int>> Handle(AddTrackToPlaylistCommand command, CancellationToken cancellationToken)
    {
        var playlist = await playlists.GetByIdAsync(new PlaylistId(command.PlaylistId), cancellationToken);
        if (playlist is null)
            return PlaylistErrors.NotFound(new PlaylistId(command.PlaylistId));

        if (playlist.UserId != new UserId(command.UserId))
            return PlaylistErrors.NotOwner;

        var result = playlist.AddTrack(new TrackId(command.TrackId), timeProvider.GetUtcNow());
        if (result.IsFailure)
            return result.Error;

        await dbContext.SaveChangesAsync(cancellationToken);
        return playlist.TrackCount;
    }
}
