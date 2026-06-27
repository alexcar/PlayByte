using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PlayByte.Api.Controllers;
using PlayByte.Application.Catalog.Playback;
using PlayByte.Domain.Common;
using Shouldly;
using Xunit;

namespace PlayByte.Api.UnitTests.Controllers;

public sealed class PlaybackControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly PlaybackController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public PlaybackControllerTests()
        => _controller = new PlaybackController(_sender).WithUser(_userId);

    [Fact]
    public async Task Play_QuandoSucesso_DeveRetornar200ComUsuarioEFaixa()
    {
        var trackId = Guid.NewGuid();
        var response = new PlaybackResponse(trackId, "One", "Metallica", "https://stream/one");
        _sender.Send(Arg.Any<PlayTrackQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        var result = await _controller.Play(trackId, CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
        await _sender.Received(1).Send(
            Arg.Is<PlayTrackQuery>(q => q.UserId == _userId && q.TrackId == trackId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Play_QuandoPlanoGratuito_DeveRetornar403()
    {
        _sender.Send(Arg.Any<PlayTrackQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PlaybackResponse>(
                Error.Forbidden("playback.requires_paid_plan", "Disponivel apenas no plano pago.")));

        var result = await _controller.Play(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task Play_QuandoFaixaNaoEncontrada_DeveRetornar404()
    {
        _sender.Send(Arg.Any<PlayTrackQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PlaybackResponse>(Error.NotFound("track.not_found", "Faixa nao encontrada.")));

        var result = await _controller.Play(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status404NotFound);
    }
}
