using AutoMapper;
using Dapper;
using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Common;
using PlayByte.Domain.Users;

namespace PlayByte.Application.Users.Queries.GetUserById;

/// <summary>
/// Lado de LEITURA do CQRS: Dapper consulta direto, sem passar pelo EF/agregado.
/// O filtro de soft delete e' explicito no SQL (is_deleted = 0).
/// </summary>
internal sealed class GetUserByIdQueryHandler(ISqlConnectionFactory connectionFactory, IMapper mapper)
    : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT  id            AS Id,
                    name          AS Name,
                    email         AS Email,
                    is_active     AS IsActive,
                    created_at_utc AS CreatedAtUtc
            FROM    users
            WHERE   id = @UserId
              AND   is_deleted = 0
            """;

        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<UserReadModel>(
            new CommandDefinition(sql, new { query.UserId }, cancellationToken: cancellationToken));

        if (row is null)
            return UserErrors.NotFound(new UserId(query.UserId));

        return mapper.Map<UserResponse>(row);
    }
}
