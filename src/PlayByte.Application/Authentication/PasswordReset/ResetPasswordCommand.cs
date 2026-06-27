using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Authentication.PasswordReset;

public sealed record ResetPasswordCommand(string Token, string NewPassword) : ICommand;
