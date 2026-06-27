using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserResponse>;
