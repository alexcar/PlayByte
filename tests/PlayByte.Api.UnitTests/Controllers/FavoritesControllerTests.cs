using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PlayByte.Api.Controllers;
using PlayByte.Application.Favorites.Bands;
using PlayByte.Application.Favorites.Tracks;
using PlayByte.Domain.Common;
using Shouldly;
using Xunit;

namespace PlayByte.Api.UnitTests.Controllers;

public sealed class FavoritesControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly FavoritesController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public FavoritesControllerTests()
        => _controller = new FavoritesController(_sender).WithUser(_userId);

    [Fact]
    public async Task ListTracks_QuandoSucesso_DeveRetornar200()
    {
        IReadOnlyList<FavoriteTrackItem> tracks = [new FavoriteTrackItem(Guid.NewGuid(), "One", "Metallica")];
        _sender.Send(Arg.Any<ListFavoriteTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(tracks));

        var result = await _controller.ListTracks(CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(tracks);
        await _sender.Received(1).Send(
            Arg.Is<ListFavoriteTracksQuery>(q => q.UserId == _userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FavoriteTrack_QuandoSucesso_DeveRetornar204()
    {
        var trackId = Guid.NewGuid();
        _sender.Send(Arg.Any<FavoriteTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _controller.FavoriteTrack(trackId, CancellationToken.None);

        result.ShouldBeOfType<NoContentResult>();
        await _sender.Received(1).Send(
            Arg.Is<FavoriteTrackCommand>(c => c.UserId == _userId && c.TrackId == trackId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FavoriteTrack_QuandoFaixaNaoEncontrada_DeveRetornar404()
    {
        _sender.Send(Arg.Any<FavoriteTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.NotFound("track.not_found", "Faixa nao encontrada.")));

        var result = await _controller.FavoriteTrack(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UnfavoriteTrack_QuandoSucesso_DeveRetornar204()
    {
        var trackId = Guid.NewGuid();
        _sender.Send(Arg.Any<UnfavoriteTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _controller.UnfavoriteTrack(trackId, CancellationToken.None);

        result.ShouldBeOfType<NoContentResult>();
        await _sender.Received(1).Send(
            Arg.Is<UnfavoriteTrackCommand>(c => c.UserId == _userId && c.TrackId == trackId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListBands_QuandoSucesso_DeveRetornar200()
    {
        IReadOnlyList<FavoriteBandItem> bands = [new FavoriteBandItem(Guid.NewGuid(), "Metallica", null)];
        _sender.Send(Arg.Any<ListFavoriteBandsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(bands));

        var result = await _controller.ListBands(CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(bands);
        await _sender.Received(1).Send(
            Arg.Is<ListFavoriteBandsQuery>(q => q.UserId == _userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FavoriteBand_QuandoSucesso_DeveRetornar204()
    {
        var bandId = Guid.NewGuid();
        _sender.Send(Arg.Any<FavoriteBandCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _controller.FavoriteBand(bandId, CancellationToken.None);

        result.ShouldBeOfType<NoContentResult>();
        await _sender.Received(1).Send(
            Arg.Is<FavoriteBandCommand>(c => c.UserId == _userId && c.BandId == bandId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnfavoriteBand_QuandoSucesso_DeveRetornar204()
    {
        var bandId = Guid.NewGuid();
        _sender.Send(Arg.Any<UnfavoriteBandCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _controller.UnfavoriteBand(bandId, CancellationToken.None);

        result.ShouldBeOfType<NoContentResult>();
        await _sender.Received(1).Send(
            Arg.Is<UnfavoriteBandCommand>(c => c.UserId == _userId && c.BandId == bandId),
            Arg.Any<CancellationToken>());
    }
}
