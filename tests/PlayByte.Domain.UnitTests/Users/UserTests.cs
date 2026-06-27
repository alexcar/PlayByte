using PlayByte.Domain.Users;
using PlayByte.Domain.Users.Events;
using PlayByte.Domain.Users.ValueObjects;
using Shouldly;
using Xunit;

namespace PlayByte.Domain.UnitTests.Users;

public sealed class UserTests
{
    private static User CriarUsuario()
    {
        var name = UserName.Create("Alexandre Dev").Value;
        var email = Email.Create("alexandre@playbyte.com").Value;
        var hash = PasswordHash.Create("$2a$12$abcdefghijklmnopqrstuv").Value;
        return User.Register(name, email, hash).Value;
    }

    [Fact]
    public void Register_DeveLevantarEventoUserRegistered()
    {
        var user = CriarUsuario();

        user.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<UserRegistered>();
    }

    [Fact]
    public void Register_DeveIniciarInativo()
    {
        var user = CriarUsuario();

        user.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Activate_QuandoInativo_DeveAtivar()
    {
        var user = CriarUsuario();

        var result = user.Activate();

        result.IsSuccess.ShouldBeTrue();
        user.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Activate_QuandoJaAtivo_DeveFalhar()
    {
        var user = CriarUsuario();
        user.Activate();

        var result = user.Activate();

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.AlreadyActive);
    }

    [Fact]
    public void MarkAsDeleted_DeveSerIdempotente()
    {
        var user = CriarUsuario();
        var now = DateTimeOffset.UtcNow;

        user.MarkAsDeleted(now);
        var primeiraData = user.DeletedAtUtc;
        user.MarkAsDeleted(now.AddHours(1));

        user.IsDeleted.ShouldBeTrue();
        user.DeletedAtUtc.ShouldBe(primeiraData);
    }
}
