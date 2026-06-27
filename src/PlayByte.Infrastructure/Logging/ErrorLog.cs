namespace PlayByte.Infrastructure.Logging;

/// <summary>Registro de excecao persistido na tabela error_logs.</summary>
public sealed class ErrorLog
{
    public Guid Id { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? Source { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int StatusCode { get; set; }
    public string? CorrelationId { get; set; }
}
