namespace PlayByte.Application.Users.Queries.GetUserById;

/// <summary>Linha crua lida pelo Dapper (snake_case mapeado por convencao no Program).</summary>
internal sealed class UserReadModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}
