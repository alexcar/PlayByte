using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Application.Users.Queries.GetUserById;

namespace PlayByte.Application.Users.Commands.UpdateProfile;

/// <summary>Atualiza nome e e-mail do perfil. Retorna o usuario atualizado.</summary>
public sealed record UpdateUserProfileCommand(Guid UserId, string Name, string Email) : ICommand<UserResponse>;
