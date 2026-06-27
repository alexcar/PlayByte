using Dapper;
using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Application.Abstractions.Pagination;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Catalog.Queries.ListBands;

internal sealed class ListBandsQueryHandler(ISqlConnectionFactory connectionFactory)
    : IQueryHandler<ListBandsQuery, PagedResult<BandListItem>>
{
    public async Task<Result<PagedResult<BandListItem>>> Handle(
        ListBandsQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var offset = (page - 1) * pageSize;

        const string sql =
            """
            SELECT count(*) FROM bands;

            SELECT id AS Id, name AS Name, cover_image_url AS CoverImageUrl
            FROM   bands
            ORDER BY name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, new { Offset = offset, PageSize = pageSize }, cancellationToken: cancellationToken));

        var total = await multi.ReadSingleAsync<long>();
        var items = (await multi.ReadAsync<BandListItem>()).ToList();

        return new PagedResult<BandListItem>(items, page, pageSize, total);
    }
}
