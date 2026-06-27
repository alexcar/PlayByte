using Dapper;
using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Catalog.Queries.GetBandDetails;

internal sealed class GetBandDetailsQueryHandler(ISqlConnectionFactory connectionFactory)
    : IQueryHandler<GetBandDetailsQuery, BandDetailsResponse>
{
    private sealed record BandRow(Guid Id, string Name, string? CoverImageUrl);
    private sealed record AlbumRow(Guid Id, string Title, int ReleaseYear);
    private sealed record TrackRow(Guid Id, Guid AlbumId, string Title, int DurationSeconds);

    public async Task<Result<BandDetailsResponse>> Handle(
        GetBandDetailsQuery query, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT id AS Id, name AS Name, cover_image_url AS CoverImageUrl
            FROM bands WHERE id = @BandId;

            SELECT id AS Id, title AS Title, release_year AS ReleaseYear
            FROM albums WHERE band_id = @BandId ORDER BY release_year;

            SELECT t.id AS Id, t.album_id AS AlbumId, t.title AS Title, t.duration_seconds AS DurationSeconds
            FROM tracks t
            JOIN albums a ON a.id = t.album_id
            WHERE a.band_id = @BandId
            ORDER BY t.title;
            """;

        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, new { query.BandId }, cancellationToken: cancellationToken));

        var band = await multi.ReadSingleOrDefaultAsync<BandRow>();
        if (band is null)
            return CatalogErrors.BandNotFound(new BandId(query.BandId));

        var albums = (await multi.ReadAsync<AlbumRow>()).ToList();
        var tracksByAlbum = (await multi.ReadAsync<TrackRow>())
            .GroupBy(t => t.AlbumId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var albumDtos = albums.Select(a => new AlbumDto(
            a.Id, a.Title, a.ReleaseYear,
            tracksByAlbum.TryGetValue(a.Id, out var ts)
                ? ts.Select(t => new TrackDto(t.Id, t.Title, t.DurationSeconds)).ToList()
                : [])).ToList();

        return new BandDetailsResponse(band.Id, band.Name, band.CoverImageUrl, albumDtos);
    }
}
