using Dapper;
using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Favorites.Bands;

public sealed record ListFavoriteBandsQuery(Guid UserId) : IQuery<IReadOnlyList<FavoriteBandItem>>;
public sealed record FavoriteBandItem(Guid BandId, string Name, string? CoverImageUrl);

internal sealed class ListFavoriteBandsQueryHandler(ISqlConnectionFactory connectionFactory)
    : IQueryHandler<ListFavoriteBandsQuery, IReadOnlyList<FavoriteBandItem>>
{
    public async Task<Result<IReadOnlyList<FavoriteBandItem>>> Handle(
        ListFavoriteBandsQuery query, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT  b.id   AS BandId,
                    b.name AS Name,
                    b.cover_image_url AS CoverImageUrl
            FROM    favorite_bands f
            JOIN    bands b ON b.id = f.band_id
            WHERE   f.user_id = @UserId
            ORDER BY f.created_at_utc DESC
            """;

        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var items = (await connection.QueryAsync<FavoriteBandItem>(
            new CommandDefinition(sql, new { query.UserId }, cancellationToken: cancellationToken))).ToList();
        return items;
    }
}
