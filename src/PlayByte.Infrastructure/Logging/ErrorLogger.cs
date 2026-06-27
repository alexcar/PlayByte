using Dapper;
using Microsoft.Extensions.Logging;
using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Logging;

namespace PlayByte.Infrastructure.Logging;

/// <summary>
/// Grava a excecao na tabela error_logs via Dapper (conexao propria do SQL Server,
/// fora do change tracker do EF, para nao depender de uma transacao que pode ter falhado).
/// Se a gravacao no banco falhar, faz fallback para o log em arquivo (Serilog),
/// registrando tanto a excecao original quanto a falha de persistencia.
/// </summary>
internal sealed class ErrorLogger(ISqlConnectionFactory connectionFactory, ILogger<ErrorLogger> logger)
    : IExceptionLogger
{
    private const string InsertSql =
        """
        INSERT INTO error_logs
            (id, occurred_at_utc, message, exception_type, stack_trace, source,
             request_path, request_method, status_code, correlation_id)
        VALUES
            (@Id, @OccurredAtUtc, @Message, @ExceptionType, @StackTrace, @Source,
             @RequestPath, @RequestMethod, @StatusCode, @CorrelationId)
        """;

    public async Task LogAsync(Exception exception, ExceptionContext context, CancellationToken ct = default)
    {
        var entry = new ErrorLog
        {
            Id = Guid.CreateVersion7(),
            OccurredAtUtc = DateTimeOffset.UtcNow,
            Message = exception.Message,
            ExceptionType = exception.GetType().FullName ?? exception.GetType().Name,
            StackTrace = exception.StackTrace,
            Source = exception.Source,
            RequestPath = context.RequestPath,
            RequestMethod = context.RequestMethod,
            StatusCode = context.StatusCode,
            CorrelationId = context.CorrelationId
        };

        try
        {
            using var connection = await connectionFactory.OpenConnectionAsync(ct);
            await connection.ExecuteAsync(new CommandDefinition(InsertSql, entry, cancellationToken: ct));
        }
        catch (Exception persistenceEx)
        {
            // FALLBACK: nao foi possivel gravar no banco -> grava em arquivo (Serilog).
            logger.LogError(exception,
                "[FALLBACK-ARQUIVO] Excecao nao persistida no banco. CorrelationId={CorrelationId}, Path={RequestPath}",
                context.CorrelationId, context.RequestPath);

            logger.LogError(persistenceEx,
                "[FALLBACK-ARQUIVO] Falha ao gravar error_logs no SQL Server para CorrelationId={CorrelationId}",
                context.CorrelationId);
        }
    }
}
