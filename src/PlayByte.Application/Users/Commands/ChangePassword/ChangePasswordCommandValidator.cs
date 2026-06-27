using FluentValidation;

namespace PlayByte.Application.Users.Commands.ChangePassword;

/// <summary>Forca da nova senha (mesmas regras do cadastro). Validacao de entrada.</summary>
public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("A senha atual e obrigatoria.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("A nova senha e obrigatoria.")
            .MinimumLength(8).WithMessage("A senha deve ter ao menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("A senha deve conter ao menos uma letra maiuscula.")
            .Matches("[a-z]").WithMessage("A senha deve conter ao menos uma letra minuscula.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um numero.");
    }
}
