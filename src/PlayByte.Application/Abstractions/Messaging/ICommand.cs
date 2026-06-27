using MediatR;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Abstractions.Messaging;

/// <summary>Comando sem retorno de valor (apenas Result).</summary>
public interface ICommand : IRequest<Result>;

/// <summary>Comando com retorno de valor (Result&lt;TResponse&gt;).</summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
