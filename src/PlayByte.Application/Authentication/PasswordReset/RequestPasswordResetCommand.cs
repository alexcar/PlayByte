using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Authentication.PasswordReset;

public sealed record RequestPasswordResetCommand(string Email) : ICommand;
