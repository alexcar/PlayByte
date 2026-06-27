using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlayByte.Domain.Common;

namespace PlayByte.Api.Extensions;

/// <summary>Traduz um Error de dominio em ProblemDetails com o status HTTP adequado.</summary>
public static class ApiResults
{
    public static IActionResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Description,
            Type = $"https://httpstatuses.io/{statusCode}"
        };

        return new ObjectResult(problem) { StatusCode = statusCode };
    }
}
