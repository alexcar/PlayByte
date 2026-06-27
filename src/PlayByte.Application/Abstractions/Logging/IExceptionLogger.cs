namespace PlayByte.Application.Abstractions.Logging;

/// <summary>Contexto minimo da requisicao no momento da excecao.</summary>
public sealed record ExceptionContext(
    string? RequestPath,
    string? RequestMethod,
    int StatusCode,
    string? CorrelationId);

/// <summary>
/// Persiste excecoes. A implementacao tenta gravar no banco; em caso de falha,
/// faz fallback para o log em arquivo (Serilog).
/// </summary>
public interface IExceptionLogger
{
    Task LogAsync(Exception exception, ExceptionContext context, CancellationToken ct = default);
}
