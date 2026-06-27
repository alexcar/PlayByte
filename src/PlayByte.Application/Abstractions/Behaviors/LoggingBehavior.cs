using MediatR;
using Microsoft.Extensions.Logging;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Abstractions.Behaviors;

/// <summary>Loga inicio/fim de cada request e o Error em caso de falha.</summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        logger.LogInformation("Processando {RequestName}", name);

        var result = await next(cancellationToken);

        if (result.IsSuccess)
            logger.LogInformation("{RequestName} concluido com sucesso", name);
        else
            logger.LogWarning("{RequestName} falhou: {ErrorCode} - {ErrorDescription}",
                name, result.Error.Code, result.Error.Description);

        return result;
    }
}
