using Dapper;
using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Search;

/// <summary>
/// US-16: busca bandas e musicas pelo termo, agrupadas por categoria. Usa LIKE com a
/// collation padrao do SQL Server (case-insensitive, US-16 c4). Para o requisito de
/// &lt; 500ms (c2), os indices em bands.name / tracks.title ajudam; em escala, considere
/// Full-Text Search do SQL Server.
/// </summary>
internal sealed class SearchQueryHandler(ISqlConnectionFactory connectionFactory)
    : IQueryHandler<SearchQuery, SearchResponse>
{
    public async Task<Result<SearchResponse>> Handle(SearchQuery query, CancellationToken cancellationToken)
    {
        var term = query.Term?.Trim() ?? string.Empty;
        if (term.Length == 0)
            return new SearchResponse([], []);

        var pattern = $"%{term}%";

        const string sql =
            """
            SELECT TOP (50) id AS Id, name AS Name, cover_image_url AS CoverImageUrl
            FROM   bands
            WHERE  name LIKE @Pattern
            ORDER BY name;

            SELECT TOP (50) t.id AS Id, t.title AS Title, b.name AS BandName
            FROM   tracks t
            JOIN   albums a ON a.id = t.album_id
            JOIN   bands  b ON b.id = a.band_id
            WHERE  t.title LIKE @Pattern
            ORDER BY t.title;
            """;

        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, new { Pattern = pattern }, cancellationToken: cancellationToken));

        var bands = (await multi.ReadAsync<SearchBandItem>()).ToList();
        var tracks = (await multi.ReadAsync<SearchTrackItem>()).ToList();

        return new SearchResponse(bands, tracks);
    }
}
