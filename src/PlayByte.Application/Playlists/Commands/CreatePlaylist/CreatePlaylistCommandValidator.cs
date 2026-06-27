using FluentValidation;

namespace PlayByte.Application.Playlists.Commands.CreatePlaylist;

public sealed class CreatePlaylistCommandValidator : AbstractValidator<CreatePlaylistCommand>
{
    public CreatePlaylistCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome da playlist e obrigatorio.")
            .MaximumLength(150);
    }
}
