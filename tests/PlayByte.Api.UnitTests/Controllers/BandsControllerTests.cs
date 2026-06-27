using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PlayByte.Api.Controllers;
using PlayByte.Application.Abstractions.Pagination;
using PlayByte.Application.Catalog.Commands.AddAlbum;
using PlayByte.Application.Catalog.Commands.AddTrack;
using PlayByte.Application.Catalog.Commands.CreateBand;
using PlayByte.Application.Catalog.Queries.GetBandDetails;
using PlayByte.Application.Catalog.Queries.ListBands;
using PlayByte.Domain.Common;
using Shouldly;
using Xunit;

namespace PlayByte.Api.UnitTests.Controllers;

public sealed class BandsControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly BandsController _controller;

    public BandsControllerTests() => _controller = new BandsController(_sender);

    [Fact]
    public async Task List_QuandoSucesso_DeveRetornar200ComPagina()
    {
        var page = new PagedResult<BandListItem>([new BandListItem(Guid.NewGuid(), "Metallica", null)], 1, 20, 1);
        _sender.Send(Arg.Any<ListBandsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(page));

        var result = await _controller.List(2, 50, CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(page);
        await _sender.Received(1).Send(
            Arg.Is<ListBandsQuery>(q => q.Page == 2 && q.PageSize == 50), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Details_QuandoSucesso_DeveRetornar200()
    {
        var band = new BandDetailsResponse(Guid.NewGuid(), "Metallica", null, []);
        _sender.Send(Arg.Any<GetBandDetailsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(band));

        var result = await _controller.Details(band.Id, CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(band);
    }

    [Fact]
    public async Task Details_QuandoNaoEncontrado_DeveRetornar404()
    {
        _sender.Send(Arg.Any<GetBandDetailsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<BandDetailsResponse>(Error.NotFound("band.not_found", "Banda nao encontrada.")));

        var result = await _controller.Details(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Create_QuandoSucesso_DeveRetornar201ParaDetails()
    {
        var id = Guid.NewGuid();
        _sender.Send(Arg.Any<CreateBandCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(id));

        var result = await _controller.Create(new CreateBandRequest("Metallica", null), CancellationToken.None);

        var created = result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe(nameof(BandsController.Details));
        created.RouteValues!["id"].ShouldBe(id);
    }

    [Fact]
    public async Task Create_QuandoInvalido_DeveRetornar400()
    {
        _sender.Send(Arg.Any<CreateBandCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Guid>(Error.Validation("band.invalid_name", "Nome invalido.")));

        var result = await _controller.Create(new CreateBandRequest("", null), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task AddAlbum_QuandoSucesso_DeveRetornar200ComId()
    {
        var bandId = Guid.NewGuid();
        var albumId = Guid.NewGuid();
        _sender.Send(Arg.Any<AddAlbumCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(albumId));

        var result = await _controller.AddAlbum(bandId, new AddAlbumRequest("Master of Puppets", 1986), CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>();
        await _sender.Received(1).Send(
            Arg.Is<AddAlbumCommand>(c => c.BandId == bandId && c.Title == "Master of Puppets" && c.ReleaseYear == 1986),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAlbum_QuandoBandaNaoEncontrada_DeveRetornar404()
    {
        _sender.Send(Arg.Any<AddAlbumCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Guid>(Error.NotFound("band.not_found", "Banda nao encontrada.")));

        var result = await _controller.AddAlbum(Guid.NewGuid(), new AddAlbumRequest("X", 2000), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task AddTrack_QuandoSucesso_DeveRetornar200ComId()
    {
        var bandId = Guid.NewGuid();
        var albumId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        _sender.Send(Arg.Any<AddTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(trackId));

        var result = await _controller.AddTrack(bandId, albumId, new AddTrackRequest("Battery", 312), CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>();
        await _sender.Received(1).Send(
            Arg.Is<AddTrackCommand>(c =>
                c.BandId == bandId && c.AlbumId == albumId && c.Title == "Battery" && c.DurationSeconds == 312),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddTrack_QuandoAlbumNaoEncontrado_DeveRetornar404()
    {
        _sender.Send(Arg.Any<AddTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Guid>(Error.NotFound("album.not_found", "Album nao encontrado.")));

        var result = await _controller.AddTrack(Guid.NewGuid(), Guid.NewGuid(), new AddTrackRequest("X", 100), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status404NotFound);
    }
}
