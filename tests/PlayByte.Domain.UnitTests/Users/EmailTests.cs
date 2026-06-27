using PlayByte.Domain.Users.ValueObjects;
using Shouldly;
using Xunit;

namespace PlayByte.Domain.UnitTests.Users;

public sealed class EmailTests
{
    [Theory]
    [InlineData("user@playbyte.com")]
    [InlineData("a.b-c@sub.dominio.com.br")]
    public void Create_ComEmailValido_DeveRetornarSucesso(string input)
    {
        var result = Email.Create(input);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(input.ToLowerInvariant());
    }

    [Fact]
    public void Create_DeveNormalizarParaMinusculasEAparar()
    {
        var result = Email.Create("  USER@PlayByte.COM  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("user@playbyte.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sem-arroba")]
    [InlineData("a@b")]
    public void Create_ComEmailInvalido_DeveRetornarFalha(string input)
    {
        var result = Email.Create(input);

        result.IsFailure.ShouldBeTrue();
    }
}
