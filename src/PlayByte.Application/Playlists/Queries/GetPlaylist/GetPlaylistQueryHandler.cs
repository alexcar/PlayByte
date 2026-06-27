using Dapper;
using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Common;
using PlayByte.Domain.Playlists;
using PlayByte.Domain.Users;

namespace PlayByte.Application.Playlists.Queries.GetPlaylist;

internal sealed class GetPlaylistQueryHandler(ISqlConnectionFactory connectionFactory)
    : IQueryHandler<GetPlaylistQuery, PlaylistDetailsResponse>
{
    private sealed record HeaderRow(Guid Id, string Name, Guid UserId);

    public async Task<Result<PlaylistDetailsResponse>> Handle(
        GetPlaylistQuery query, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT id AS Id, name AS Name, user_id AS UserId
            FROM playlists WHERE id = @PlaylistId;

            SELECT  pi.track_id AS TrackId,
                    t.title     AS Title,
                    b.name      AS BandName,
                    pi.position AS Position
            FROM    playlist_items pi
            JOIN    tracks t ON t.id = pi.track_id
            JOIN    albums a ON a.id = t.album_id
            JOIN    bands  b ON b.id = a.band_id
            WHERE   pi.playlist_id = @PlaylistId
            ORDER BY pi.position;
            """;

        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, new { query.PlaylistId }, cancellationToken: cancellationToken));

        var header = await multi.ReadSingleOrDefaultAsync<HeaderRow>();
        if (header is null)
            return PlaylistErrors.NotFound(new PlaylistId(query.PlaylistId));
        if (header.UserId != query.UserId)
            return PlaylistErrors.NotOwner;

        var tracks = (await multi.ReadAsync<PlaylistTrack>()).ToList();
        return new PlaylistDetailsResponse(header.Id, header.Name, tracks.Count, tracks);
    }
}
