using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PlayByte.Api.Controllers;
using PlayByte.Application.Authentication.Login;
using PlayByte.Application.Authentication.PasswordReset;
using PlayByte.Domain.Common;
using Shouldly;
using Xunit;

namespace PlayByte.Api.UnitTests.Controllers;

public sealed class AuthControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly AuthController _controller;

    public AuthControllerTests() => _controller = new AuthController(_sender);

    [Fact]
    public async Task Login_QuandoSucesso_DeveRetornar200ComToken()
    {
        var response = new LoginResponse(Guid.NewGuid(), "token-jwt", DateTimeOffset.UtcNow.AddHours(1));
        _sender.Send(Arg.Any<LoginQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        var result = await _controller.Login(new LoginRequest("a@playbyte.com", "Senha@123"), CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact]
    public async Task Login_QuandoCredenciaisInvalidas_DeveRetornar401()
    {
        _sender.Send(Arg.Any<LoginQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<LoginResponse>(Error.Unauthorized("auth.invalid", "Credenciais invalidas.")));

        var result = await _controller.Login(new LoginRequest("a@playbyte.com", "errada"), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task ForgotPassword_DeveSempreRetornar200SemEnumerarContas()
    {
        _sender.Send(Arg.Any<RequestPasswordResetCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _controller.ForgotPassword(new ForgotPasswordRequest("a@playbyte.com"), CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>();
        await _sender.Received(1).Send(
            Arg.Is<RequestPasswordResetCommand>(c => c.Email == "a@playbyte.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResetPassword_QuandoSucesso_DeveRetornar200()
    {
        _sender.Send(Arg.Any<ResetPasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _controller.ResetPassword(new ResetPasswordRequest("token", "Nova@123"), CancellationToken.None);

        result.ShouldBeOfType<OkResult>();
    }

    [Fact]
    public async Task ResetPassword_QuandoTokenInvalido_DeveRetornar400()
    {
        _sender.Send(Arg.Any<ResetPasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Validation("auth.invalid_token", "Token invalido ou expirado.")));

        var result = await _controller.ResetPassword(new ResetPasswordRequest("ruim", "Nova@123"), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status400BadRequest);
    }
}
