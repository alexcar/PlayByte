using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayByte.Api.Extensions;
using PlayByte.Application.Search;

namespace PlayByte.Api.Controllers;

[ApiController]
[Route("api/search")]
[Produces("application/json")]
[Authorize]
public sealed class SearchController(ISender sender) : ControllerBase
{
    /// <summary>Busca bandas e musicas (US-16). Disponivel para ambos os planos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(SearchResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        var result = await sender.Send(new SearchQuery(q), ct);
        return result.IsSuccess ? Ok(result.Value) : ApiResults.Problem(result.Error);
    }
}
