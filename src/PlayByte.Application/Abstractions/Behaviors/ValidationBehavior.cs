using FluentValidation;
using MediatR;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Abstractions.Behaviors;

/// <summary>
/// Roda os validadores FluentValidation antes do handler. Em caso de falha,
/// curto-circuita retornando um Result de falha (sem lancar excecao).
/// Restrito a TResponse : Result para poder construir a falha genericamente.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToArray();

        if (failures.Length == 0)
            return await next(cancellationToken);

        var description = string.Join(" | ", failures.Select(f => f.ErrorMessage).Distinct());
        var error = Error.Validation("Validation.Failed", description);

        return CreateFailureResult(error);
    }

    private static TResponse CreateFailureResult(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        // Result<T>: invoca Result.Failure<T>(error) via reflexao.
        var valueType = typeof(TResponse).GetGenericArguments()[0];
        var failureMethod = typeof(Result)
            .GetMethods()
            .First(m => m is { Name: nameof(Result.Failure), IsGenericMethod: true })
            .MakeGenericMethod(valueType);

        return (TResponse)failureMethod.Invoke(null, [error])!;
    }
}
