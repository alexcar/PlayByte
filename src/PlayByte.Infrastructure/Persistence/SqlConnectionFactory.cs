using System.Data;
using Microsoft.Data.SqlClient;
using PlayByte.Application.Abstractions.Data;

namespace PlayByte.Infrastructure.Persistence;

/// <summary>Abre conexoes do SQL Server para o lado de leitura (Dapper).</summary>
internal sealed class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public async Task<IDbConnection> OpenConnectionAsync(CancellationToken ct = default)
    {
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        return connection;
    }
}
