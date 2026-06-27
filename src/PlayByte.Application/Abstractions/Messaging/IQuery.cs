using MediatR;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Abstractions.Messaging;

/// <summary>Query sempre retorna Result&lt;TResponse&gt; (permite NotFound etc.).</summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
