using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Users.Commands.ChangePassword;

public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : ICommand;
