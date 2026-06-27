using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Common;
using PlayByte.Domain.Playlists;
using PlayByte.Domain.Users;

namespace PlayByte.Application.Playlists.Commands.RemoveTrackFromPlaylist;

/// <summary>Remove uma faixa da playlist (US-13); retorna a quantidade restante.</summary>
internal sealed class RemoveTrackFromPlaylistCommandHandler(
    IPlaylistRepository playlists, IApplicationDbContext dbContext)
    : ICommandHandler<RemoveTrackFromPlaylistCommand, int>
{
    public async Task<Result<int>> Handle(RemoveTrackFromPlaylistCommand command, CancellationToken cancellationToken)
    {
        var playlist = await playlists.GetByIdAsync(new PlaylistId(command.PlaylistId), cancellationToken);
        if (playlist is null)
            return PlaylistErrors.NotFound(new PlaylistId(command.PlaylistId));

        if (playlist.UserId != new UserId(command.UserId))
            return PlaylistErrors.NotOwner;

        var result = playlist.RemoveTrack(new TrackId(command.TrackId));
        if (result.IsFailure)
            return result.Error;

        await dbContext.SaveChangesAsync(cancellationToken);
        return playlist.TrackCount;
    }
}
