using FluentValidation;

namespace PlayByte.Application.Users.Commands.RegisterUser;

/// <summary>
/// Validacao de ENTRADA (formato/forca). A forca da senha pertence aqui,
/// NUNCA no dominio (a senha em texto plano nao entra no agregado).
/// </summary>
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome e obrigatorio.")
            .MinimumLength(3).MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail e obrigatorio.")
            .EmailAddress().WithMessage("O e-mail e invalido.")
            .MaximumLength(254);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha e obrigatoria.")
            .MinimumLength(8).WithMessage("A senha deve ter ao menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("A senha deve conter ao menos uma letra maiuscula.")
            .Matches("[a-z]").WithMessage("A senha deve conter ao menos uma letra minuscula.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um numero.");
    }
}
