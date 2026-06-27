using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PlayByte.Api.Controllers;
using PlayByte.Application.Search;
using PlayByte.Domain.Common;
using Shouldly;
using Xunit;

namespace PlayByte.Api.UnitTests.Controllers;

public sealed class SearchControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly SearchController _controller;

    public SearchControllerTests() => _controller = new SearchController(_sender);

    [Fact]
    public async Task Search_QuandoSucesso_DeveRetornar200ComResultados()
    {
        var response = new SearchResponse(
            [new SearchBandItem(Guid.NewGuid(), "Metallica", null)],
            [new SearchTrackItem(Guid.NewGuid(), "One", "Metallica")]);
        _sender.Send(Arg.Any<SearchQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        var result = await _controller.Search("metal", CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
        await _sender.Received(1).Send(
            Arg.Is<SearchQuery>(q => q.Term == "metal"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Search_DeveRepassarOTermoVazioParaOHandler()
    {
        _sender.Send(Arg.Any<SearchQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new SearchResponse([], [])));

        var result = await _controller.Search("", CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>();
        await _sender.Received(1).Send(Arg.Is<SearchQuery>(q => q.Term == ""), Arg.Any<CancellationToken>());
    }
}
