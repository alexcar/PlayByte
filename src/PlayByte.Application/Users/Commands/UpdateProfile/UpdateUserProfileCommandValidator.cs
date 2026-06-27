using FluentValidation;

namespace PlayByte.Application.Users.Commands.UpdateProfile;

public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome e obrigatorio.")
            .MinimumLength(3).MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail e obrigatorio.")
            .EmailAddress().WithMessage("O e-mail e invalido.")
            .MaximumLength(254);
    }
}
