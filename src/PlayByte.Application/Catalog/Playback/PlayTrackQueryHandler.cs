using Dapper;
using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Catalog.Playback;

/// <summary>
/// US-11: so libera a reproducao para quem tem plano pago ativo. Caso contrario,
/// devolve 403 convidando a assinar (US-11 c2). A "reproducao" aqui retorna a URL
/// de streaming; a entrega real do audio fica a cargo do servidor de midia.
/// </summary>
internal sealed class PlayTrackQueryHandler(ISqlConnectionFactory connectionFactory, TimeProvider timeProvider)
    : IQueryHandler<PlayTrackQuery, PlaybackResponse>
{
    private sealed record TrackInfo(Guid TrackId, string Title, string BandName);

    public async Task<Result<PlaybackResponse>> Handle(PlayTrackQuery query, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        const string trackSql =
            """
            SELECT t.id AS TrackId, t.title AS Title, b.name AS BandName
            FROM tracks t
            JOIN albums a ON a.id = t.album_id
            JOIN bands  b ON b.id = a.band_id
            WHERE t.id = @TrackId
            """;

        // Acesso pago = assinatura com status que concede acesso E vigencia nao expirada.
        const string accessSql =
            """
            SELECT CAST(
                CASE WHEN EXISTS(
                    SELECT 1 FROM subscriptions
                    WHERE user_id = @UserId
                      AND status IN (2, 3, 4, 5)        -- Trialing, Active, PastDue, Canceled
                      AND current_period_end > @Today
                ) THEN 1 ELSE 0 END AS bit)
            """;

        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var track = await connection.QuerySingleOrDefaultAsync<TrackInfo>(
            new CommandDefinition(trackSql, new { query.TrackId }, cancellationToken: cancellationToken));
        if (track is null)
            return CatalogErrors.TrackNotFound(new TrackId(query.TrackId));

        var hasAccess = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(accessSql, new { query.UserId, Today = today }, cancellationToken: cancellationToken));
        if (!hasAccess)
            return PlaybackErrors.RequiresPaidPlan;

        var streamUrl = $"https://stream.playbyte.com/tracks/{track.TrackId}";
        return new PlaybackResponse(track.TrackId, track.Title, track.BandName, streamUrl);
    }
}
