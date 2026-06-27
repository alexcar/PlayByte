using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(string Name, string Email, string Password) : ICommand<Guid>;
