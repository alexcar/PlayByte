using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayByte.Api.Extensions;
using PlayByte.Application.Favorites.Bands;
using PlayByte.Application.Favorites.Tracks;

namespace PlayByte.Api.Controllers;

[ApiController]
[Route("api/favorites")]
[Produces("application/json")]
[Authorize]
public sealed class FavoritesController(ISender sender) : ControllerBase
{
    // ---- Musicas (US-14) ----

    [HttpGet("tracks")]
    public async Task<IActionResult> ListTracks(CancellationToken ct)
    {
        var result = await sender.Send(new ListFavoriteTracksQuery(User.GetUserId()), ct);
        return result.IsSuccess ? Ok(result.Value) : ApiResults.Problem(result.Error);
    }

    [HttpPost("tracks/{trackId:guid}")]
    public async Task<IActionResult> FavoriteTrack(Guid trackId, CancellationToken ct)
    {
        var result = await sender.Send(new FavoriteTrackCommand(User.GetUserId(), trackId), ct);
        return result.IsSuccess ? NoContent() : ApiResults.Problem(result.Error);
    }

    [HttpDelete("tracks/{trackId:guid}")]
    public async Task<IActionResult> UnfavoriteTrack(Guid trackId, CancellationToken ct)
    {
        var result = await sender.Send(new UnfavoriteTrackCommand(User.GetUserId(), trackId), ct);
        return result.IsSuccess ? NoContent() : ApiResults.Problem(result.Error);
    }

    // ---- Bandas (US-15) ----

    [HttpGet("bands")]
    public async Task<IActionResult> ListBands(CancellationToken ct)
    {
        var result = await sender.Send(new ListFavoriteBandsQuery(User.GetUserId()), ct);
        return result.IsSuccess ? Ok(result.Value) : ApiResults.Problem(result.Error);
    }

    [HttpPost("bands/{bandId:guid}")]
    public async Task<IActionResult> FavoriteBand(Guid bandId, CancellationToken ct)
    {
        var result = await sender.Send(new FavoriteBandCommand(User.GetUserId(), bandId), ct);
        return result.IsSuccess ? NoContent() : ApiResults.Problem(result.Error);
    }

    [HttpDelete("bands/{bandId:guid}")]
    public async Task<IActionResult> UnfavoriteBand(Guid bandId, CancellationToken ct)
    {
        var result = await sender.Send(new UnfavoriteBandCommand(User.GetUserId(), bandId), ct);
        return result.IsSuccess ? NoContent() : ApiResults.Problem(result.Error);
    }
}
