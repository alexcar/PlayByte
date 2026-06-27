using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Common;
using PlayByte.Domain.Playlists;
using PlayByte.Domain.Users;

namespace PlayByte.Application.Playlists.Commands.CreatePlaylist;

internal sealed class CreatePlaylistCommandHandler(IPlaylistRepository playlists, IApplicationDbContext dbContext)
    : ICommandHandler<CreatePlaylistCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePlaylistCommand command, CancellationToken cancellationToken)
    {
        var userId = new UserId(command.UserId);

        var result = Playlist.Create(userId, command.Name);
        if (result.IsFailure) return result.Error;

        // Nao permite duas playlists com o mesmo nome para o mesmo usuario.
        if (await playlists.NameExistsForUserAsync(userId, result.Value.Name, cancellationToken))
            return PlaylistErrors.NameAlreadyExists;

        playlists.Add(result.Value);
        await dbContext.SaveChangesAsync(cancellationToken);
        return result.Value.Id.Value;
    }
}
