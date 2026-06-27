using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PlayByte.Api.Controllers;
using PlayByte.Application.Users.Commands.RegisterUser;
using PlayByte.Application.Users.Queries.GetUserById;
using PlayByte.Domain.Common;
using Shouldly;
using Xunit;

namespace PlayByte.Api.UnitTests.Controllers;

public sealed class UsersControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly UsersController _controller;

    public UsersControllerTests() => _controller = new UsersController(_sender);

    [Fact]
    public async Task Register_QuandoSucesso_DeveRetornar201ComLocationParaGetById()
    {
        var id = Guid.NewGuid();
        var request = new RegisterUserRequest("Alexandre", "alexandre@playbyte.com", "Senha@123");
        _sender.Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(id));

        var result = await _controller.Register(request, CancellationToken.None);

        var created = result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe(nameof(UsersController.GetById));
        created.Value.ShouldBe(id);
        created.RouteValues!["id"].ShouldBe(id);
    }

    [Fact]
    public async Task Register_DeveMapearOsCamposDoRequestParaOComando()
    {
        _sender.Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(Guid.NewGuid()));
        var request = new RegisterUserRequest("Maria", "maria@playbyte.com", "Senha@123");

        await _controller.Register(request, CancellationToken.None);

        await _sender.Received(1).Send(
            Arg.Is<RegisterUserCommand>(c =>
                c.Name == "Maria" && c.Email == "maria@playbyte.com" && c.Password == "Senha@123"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_QuandoConflito_DeveRetornar409()
    {
        var request = new RegisterUserRequest("Alexandre", "dup@playbyte.com", "Senha@123");
        _sender.Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Guid>(Error.Conflict("user.email_taken", "E-mail ja cadastrado.")));

        var result = await _controller.Register(request, CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task GetById_QuandoSucesso_DeveRetornar200ComUsuario()
    {
        var response = new UserResponse(Guid.NewGuid(), "Alexandre", "a@playbyte.com", true, DateTimeOffset.UtcNow);
        _sender.Send(Arg.Any<GetUserByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        var result = await _controller.GetById(response.Id, CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact]
    public async Task GetById_QuandoNaoEncontrado_DeveRetornar404()
    {
        _sender.Send(Arg.Any<GetUserByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UserResponse>(Error.NotFound("user.not_found", "Usuario nao encontrado.")));

        var result = await _controller.GetById(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status404NotFound);
    }
}
