using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PlayByte.Api.Controllers;
using PlayByte.Application.Playlists.Commands.AddTrackToPlaylist;
using PlayByte.Application.Playlists.Commands.CreatePlaylist;
using PlayByte.Application.Playlists.Queries.GetPlaylist;
using PlayByte.Application.Playlists.Queries.GetUserPlaylists;
using PlayByte.Domain.Common;
using Shouldly;
using Xunit;

namespace PlayByte.Api.UnitTests.Controllers;

public sealed class PlaylistsControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly PlaylistsController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public PlaylistsControllerTests()
        => _controller = new PlaylistsController(_sender).WithUser(_userId);

    [Fact]
    public async Task Create_QuandoSucesso_DeveRetornar201ParaGet()
    {
        var id = Guid.NewGuid();
        _sender.Send(Arg.Any<CreatePlaylistCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(id));

        var result = await _controller.Create(new CreatePlaylistRequest("Favoritas"), CancellationToken.None);

        var created = result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe(nameof(PlaylistsController.Get));
        created.RouteValues!["id"].ShouldBe(id);
        await _sender.Received(1).Send(
            Arg.Is<CreatePlaylistCommand>(c => c.UserId == _userId && c.Name == "Favoritas"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_QuandoInvalido_DeveRetornar400()
    {
        _sender.Send(Arg.Any<CreatePlaylistCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Guid>(Error.Validation("playlist.invalid_name", "Nome invalido.")));

        var result = await _controller.Create(new CreatePlaylistRequest(""), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task List_QuandoSucesso_DeveRetornar200()
    {
        IReadOnlyList<PlaylistSummary> playlists = [new PlaylistSummary(Guid.NewGuid(), "Favoritas", 3)];
        _sender.Send(Arg.Any<GetUserPlaylistsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(playlists));

        var result = await _controller.List(CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(playlists);
        await _sender.Received(1).Send(
            Arg.Is<GetUserPlaylistsQuery>(q => q.UserId == _userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Get_QuandoSucesso_DeveRetornar200()
    {
        var id = Guid.NewGuid();
        var response = new PlaylistDetailsResponse(id, "Favoritas", 0, []);
        _sender.Send(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        var result = await _controller.Get(id, CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
        await _sender.Received(1).Send(
            Arg.Is<GetPlaylistQuery>(q => q.UserId == _userId && q.PlaylistId == id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Get_QuandoNaoEhDono_DeveRetornar403()
    {
        _sender.Send(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PlaylistDetailsResponse>(
                Error.Forbidden("playlist.not_owner", "Playlist de outro usuario.")));

        var result = await _controller.Get(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task AddTrack_QuandoSucesso_DeveRetornar200ComContagem()
    {
        var playlistId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        _sender.Send(Arg.Any<AddTrackToPlaylistCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(5));

        var result = await _controller.AddTrack(playlistId, new AddTrackToPlaylistRequest(trackId), CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>();
        await _sender.Received(1).Send(
            Arg.Is<AddTrackToPlaylistCommand>(c =>
                c.UserId == _userId && c.PlaylistId == playlistId && c.TrackId == trackId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddTrack_QuandoDuplicada_DeveRetornar409()
    {
        _sender.Send(Arg.Any<AddTrackToPlaylistCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<int>(Error.Conflict("playlist.duplicate_track", "Faixa ja na playlist.")));

        var result = await _controller.AddTrack(Guid.NewGuid(), new AddTrackToPlaylistRequest(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status409Conflict);
    }
}
