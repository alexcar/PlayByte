using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PlayByte.Application.Abstractions.Logging;

namespace PlayByte.Api.Filters;

/// <summary>
/// Filtro global de excecoes: registra a excecao (banco com fallback para arquivo,
/// via IExceptionLogger) e devolve um ProblemDetails 500 padronizado.
/// </summary>
public sealed class ApiExceptionFilterAttribute(IExceptionLogger exceptionLogger)
    : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(Microsoft.AspNetCore.Mvc.Filters.ExceptionContext context)
    {
        var http = context.HttpContext;
        var correlationId = Activity.Current?.Id ?? http.TraceIdentifier;

        var ctx = new PlayByte.Application.Abstractions.Logging.ExceptionContext(
            RequestPath: http.Request.Path,
            RequestMethod: http.Request.Method,
            StatusCode: StatusCodes.Status500InternalServerError,
            CorrelationId: correlationId);

        await exceptionLogger.LogAsync(context.Exception, ctx, http.RequestAborted);

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Erro interno do servidor",
            Detail = "Ocorreu um erro inesperado. A equipe foi notificada.",
            Type = "https://httpstatuses.io/500"
        };
        problem.Extensions["correlationId"] = correlationId;

        context.Result = new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
        context.ExceptionHandled = true;
    }
}
