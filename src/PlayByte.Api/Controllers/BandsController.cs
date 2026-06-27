using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayByte.Api.Extensions;
using PlayByte.Application.Catalog.Commands.AddAlbum;
using PlayByte.Application.Catalog.Commands.AddTrack;
using PlayByte.Application.Catalog.Commands.CreateBand;
using PlayByte.Application.Catalog.Queries.GetBandDetails;
using PlayByte.Application.Catalog.Queries.ListBands;

namespace PlayByte.Api.Controllers;

[ApiController]
[Route("api/bands")]
[Produces("application/json")]
[Authorize]
public sealed class BandsController(ISender sender) : ControllerBase
{
    /// <summary>Lista bandas paginadas (US-09). Disponivel para plano gratuito ou pago.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await sender.Send(new ListBandsQuery(page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : ApiResults.Problem(result.Error);
    }

    /// <summary>Detalhes da banda com albuns e faixas (US-10).</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetBandDetailsQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : ApiResults.Problem(result.Error);
    }

    // ---- Cadastro de catalogo (operacao administrativa; RBAC fica como evolucao) ----

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBandRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateBandCommand(request.Name, request.CoverImageUrl), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Details), new { id = result.Value }, new { id = result.Value })
            : ApiResults.Problem(result.Error);
    }

    [HttpPost("{bandId:guid}/albums")]
    public async Task<IActionResult> AddAlbum(Guid bandId, [FromBody] AddAlbumRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AddAlbumCommand(bandId, request.Title, request.ReleaseYear), ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : ApiResults.Problem(result.Error);
    }

    [HttpPost("{bandId:guid}/albums/{albumId:guid}/tracks")]
    public async Task<IActionResult> AddTrack(Guid bandId, Guid albumId, [FromBody] AddTrackRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AddTrackCommand(bandId, albumId, request.Title, request.DurationSeconds), ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : ApiResults.Problem(result.Error);
    }
}

public sealed record CreateBandRequest(string Name, string? CoverImageUrl);
public sealed record AddAlbumRequest(string Title, int ReleaseYear);
public sealed record AddTrackRequest(string Title, int DurationSeconds);
