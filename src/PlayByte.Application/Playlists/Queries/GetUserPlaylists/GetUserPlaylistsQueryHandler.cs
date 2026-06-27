using Dapper;
using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Playlists.Queries.GetUserPlaylists;

internal sealed class GetUserPlaylistsQueryHandler(ISqlConnectionFactory connectionFactory)
    : IQueryHandler<GetUserPlaylistsQuery, IReadOnlyList<PlaylistSummary>>
{
    public async Task<Result<IReadOnlyList<PlaylistSummary>>> Handle(
        GetUserPlaylistsQuery query, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT  p.id   AS Id,
                    p.name AS Name,
                    count(pi.id) AS TrackCount
            FROM    playlists p
            LEFT JOIN playlist_items pi ON pi.playlist_id = p.id
            WHERE   p.user_id = @UserId
            GROUP BY p.id, p.name, p.created_at_utc
            ORDER BY p.created_at_utc DESC
            """;

        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var items = (await connection.QueryAsync<PlaylistSummary>(
            new CommandDefinition(sql, new { query.UserId }, cancellationToken: cancellationToken))).ToList();

        return items;
    }
}
