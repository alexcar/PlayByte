using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayByte.Api.Extensions;
using PlayByte.Application.Catalog.Playback;

namespace PlayByte.Api.Controllers;

[ApiController]
[Route("api/tracks")]
[Produces("application/json")]
[Authorize]
public sealed class PlaybackController(ISender sender) : ControllerBase
{
    /// <summary>Reproduz uma faixa (US-11). Exclusivo do plano pago: 403 para gratuito.</summary>
    [HttpPost("{id:guid}/play")]
    [ProducesResponseType(typeof(PlaybackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Play(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new PlayTrackQuery(User.GetUserId(), id), ct);
        return result.IsSuccess ? Ok(result.Value) : ApiResults.Problem(result.Error);
    }
}
