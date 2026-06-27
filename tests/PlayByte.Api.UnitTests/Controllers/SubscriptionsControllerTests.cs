using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PlayByte.Api.Controllers;
using PlayByte.Application.Billing.Subscriptions.Commands.CancelSubscription;
using PlayByte.Application.Billing.Subscriptions.Commands.SubscribeToPlan;
using PlayByte.Application.Billing.Subscriptions.Queries.GetMySubscription;
using PlayByte.Domain.Common;
using Shouldly;
using Xunit;

namespace PlayByte.Api.UnitTests.Controllers;

public sealed class SubscriptionsControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly SubscriptionsController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public SubscriptionsControllerTests()
        => _controller = new SubscriptionsController(_sender).WithUser(_userId);

    [Fact]
    public async Task Subscribe_QuandoSucesso_DeveRetornar200EUsarOUsuarioAutenticado()
    {
        var response = new SubscribeToPlanResponse(Guid.NewGuid(), Guid.NewGuid());
        _sender.Send(Arg.Any<SubscribeToPlanCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        var result = await _controller.Subscribe(new SubscribeRequest("PREMIUM", "CreditCard"), CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
        await _sender.Received(1).Send(
            Arg.Is<SubscribeToPlanCommand>(c =>
                c.UserId == _userId && c.PlanCode == "PREMIUM" && c.PaymentMethod == "CreditCard"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Subscribe_QuandoInvalido_DeveRetornar400()
    {
        _sender.Send(Arg.Any<SubscribeToPlanCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<SubscribeToPlanResponse>(Error.Validation("plan.invalid", "Plano invalido.")));

        var result = await _controller.Subscribe(new SubscribeRequest("X", "Y"), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Me_QuandoSucesso_DeveRetornar200()
    {
        var response = new MySubscriptionResponse(
            Guid.NewGuid(), "Premium", "Active", DateOnly.FromDateTime(DateTime.UtcNow), true);
        _sender.Send(Arg.Any<GetMySubscriptionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        var result = await _controller.Me(CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
        await _sender.Received(1).Send(
            Arg.Is<GetMySubscriptionQuery>(q => q.UserId == _userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Me_QuandoSemAssinatura_DeveRetornar404()
    {
        _sender.Send(Arg.Any<GetMySubscriptionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<MySubscriptionResponse>(
                Error.NotFound("subscription.none", "Sem assinatura ativa.")));

        var result = await _controller.Me(CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Cancel_QuandoSucesso_DeveRetornar204()
    {
        _sender.Send(Arg.Any<CancelSubscriptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _controller.Cancel(CancellationToken.None);

        result.ShouldBeOfType<NoContentResult>();
        await _sender.Received(1).Send(
            Arg.Is<CancelSubscriptionCommand>(c => c.UserId == _userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancel_QuandoSemAssinatura_DeveRetornar404()
    {
        _sender.Send(Arg.Any<CancelSubscriptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.NotFound("subscription.none", "Sem assinatura ativa.")));

        var result = await _controller.Cancel(CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status404NotFound);
    }
}
