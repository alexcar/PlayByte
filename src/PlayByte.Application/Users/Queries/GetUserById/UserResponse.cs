namespace PlayByte.Application.Users.Queries.GetUserById;

/// <summary>DTO publico retornado pela API.</summary>
public sealed record UserResponse(Guid Id, string Name, string Email, bool IsActive, DateTimeOffset CreatedAtUtc);
