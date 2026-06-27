using Dapper;
using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Favorites.Tracks;

public sealed record ListFavoriteTracksQuery(Guid UserId) : IQuery<IReadOnlyList<FavoriteTrackItem>>;
public sealed record FavoriteTrackItem(Guid TrackId, string Title, string BandName);

internal sealed class ListFavoriteTracksQueryHandler(ISqlConnectionFactory connectionFactory)
    : IQueryHandler<ListFavoriteTracksQuery, IReadOnlyList<FavoriteTrackItem>>
{
    public async Task<Result<IReadOnlyList<FavoriteTrackItem>>> Handle(
        ListFavoriteTracksQuery query, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT  t.id   AS TrackId,
                    t.title AS Title,
                    b.name  AS BandName
            FROM    favorite_tracks f
            JOIN    tracks t ON t.id = f.track_id
            JOIN    albums a ON a.id = t.album_id
            JOIN    bands  b ON b.id = a.band_id
            WHERE   f.user_id = @UserId
            ORDER BY f.created_at_utc DESC
            """;

        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var items = (await connection.QueryAsync<FavoriteTrackItem>(
            new CommandDefinition(sql, new { query.UserId }, cancellationToken: cancellationToken))).ToList();
        return items;
    }
}
