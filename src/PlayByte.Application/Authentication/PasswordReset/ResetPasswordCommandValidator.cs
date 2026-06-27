using FluentValidation;

namespace PlayByte.Application.Authentication.PasswordReset;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8).WithMessage("A senha deve ter ao menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("A senha deve conter ao menos uma letra maiuscula.")
            .Matches("[a-z]").WithMessage("A senha deve conter ao menos uma letra minuscula.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um numero.");
    }
}
