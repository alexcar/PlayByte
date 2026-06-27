using System.Data;

namespace PlayByte.Application.Abstractions.Data;

/// <summary>Fornece conexoes abertas para as LEITURAS via Dapper (lado de query do CQRS).</summary>
public interface ISqlConnectionFactory
{
    Task<IDbConnection> OpenConnectionAsync(CancellationToken ct = default);
}
