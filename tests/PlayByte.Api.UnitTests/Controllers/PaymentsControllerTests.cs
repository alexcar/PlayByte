using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PlayByte.Api.Controllers;
using PlayByte.Application.Billing.Payments.Commands.ApprovePayment;
using PlayByte.Domain.Common;
using Shouldly;
using Xunit;

namespace PlayByte.Api.UnitTests.Controllers;

public sealed class PaymentsControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly PaymentsController _controller;

    public PaymentsControllerTests() => _controller = new PaymentsController(_sender);

    [Fact]
    public async Task Approve_QuandoSucesso_DeveRetornar200EMapearOComando()
    {
        var paymentId = Guid.NewGuid();
        _sender.Send(Arg.Any<ApprovePaymentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _controller.Approve(paymentId, new ApprovePaymentRequest("tx-123"), CancellationToken.None);

        result.ShouldBeOfType<OkResult>();
        await _sender.Received(1).Send(
            Arg.Is<ApprovePaymentCommand>(c => c.PaymentId == paymentId && c.GatewayTransactionId == "tx-123"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Approve_QuandoPagamentoNaoEncontrado_DeveRetornar404()
    {
        _sender.Send(Arg.Any<ApprovePaymentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.NotFound("payment.not_found", "Pagamento nao encontrado.")));

        var result = await _controller.Approve(Guid.NewGuid(), new ApprovePaymentRequest("tx"), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Approve_QuandoConflito_DeveRetornar409()
    {
        _sender.Send(Arg.Any<ApprovePaymentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Conflict("payment.already_processed", "Pagamento ja processado.")));

        var result = await _controller.Approve(Guid.NewGuid(), new ApprovePaymentRequest("tx"), CancellationToken.None);

        result.ShouldBeProblem(StatusCodes.Status409Conflict);
    }
}
