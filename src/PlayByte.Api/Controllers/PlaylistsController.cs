using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayByte.Api.Extensions;
using PlayByte.Application.Playlists.Commands.AddTrackToPlaylist;
using PlayByte.Application.Playlists.Commands.CreatePlaylist;
using PlayByte.Application.Playlists.Commands.RemoveTrackFromPlaylist;
using PlayByte.Application.Playlists.Queries.GetPlaylist;
using PlayByte.Application.Playlists.Queries.GetUserPlaylists;

namespace PlayByte.Api.Controllers;

[ApiController]
[Route("api/playlists")]
[Produces("application/json")]
[Authorize]
public sealed class PlaylistsController(ISender sender) : ControllerBase
{
    /// <summary>Cria uma playlist (US-12). Disponivel para ambos os planos.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlaylistRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreatePlaylistCommand(User.GetUserId(), request.Name), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, new { id = result.Value })
            : ApiResults.Problem(result.Error);
    }

    /// <summary>Lista as playlists do usuario (US-12 c1).</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await sender.Send(new GetUserPlaylistsQuery(User.GetUserId()), ct);
        return result.IsSuccess ? Ok(result.Value) : ApiResults.Problem(result.Error);
    }

    /// <summary>Detalhe da playlist com suas faixas.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetPlaylistQuery(User.GetUserId(), id), ct);
        return result.IsSuccess ? Ok(result.Value) : ApiResults.Problem(result.Error);
    }

    /// <summary>Adiciona uma faixa a playlist (US-13). Rejeita duplicatas (409).</summary>
    [HttpPost("{id:guid}/tracks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddTrack(Guid id, [FromBody] AddTrackToPlaylistRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new AddTrackToPlaylistCommand(User.GetUserId(), id, request.TrackId), ct);

        return result.IsSuccess
            ? Ok(new { trackCount = result.Value })
            : ApiResults.Problem(result.Error);
    }

    /// <summary>Remove uma faixa da playlist (US-13).</summary>
    [HttpDelete("{id:guid}/tracks/{trackId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTrack(Guid id, Guid trackId, CancellationToken ct)
    {
        var result = await sender.Send(
            new RemoveTrackFromPlaylistCommand(User.GetUserId(), id, trackId), ct);

        return result.IsSuccess
            ? Ok(new { trackCount = result.Value })
            : ApiResults.Problem(result.Error);
    }
}

public sealed record CreatePlaylistRequest(string Name);
public sealed record AddTrackToPlaylistRequest(Guid TrackId);
