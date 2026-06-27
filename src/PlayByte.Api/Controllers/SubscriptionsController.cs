using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayByte.Api.Extensions;
using PlayByte.Application.Billing.Subscriptions.Commands.CancelSubscription;
using PlayByte.Application.Billing.Subscriptions.Commands.SubscribeToPlan;
using PlayByte.Application.Billing.Subscriptions.Queries.GetMySubscription;

namespace PlayByte.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
[Produces("application/json")]
[Authorize]
public sealed class SubscriptionsController(ISender sender) : ControllerBase
{
    /// <summary>Assina um plano pago (US-05). Cria a assinatura pendente e o pagamento.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SubscribeToPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request, CancellationToken ct)
    {
        var command = new SubscribeToPlanCommand(User.GetUserId(), request.PlanCode, request.PaymentMethod);
        var result = await sender.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : ApiResults.Problem(result.Error);
    }

    /// <summary>Plano atual do usuario (US-05 c3).</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(MySubscriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var result = await sender.Send(new GetMySubscriptionQuery(User.GetUserId()), ct);
        return result.IsSuccess ? Ok(result.Value) : ApiResults.Problem(result.Error);
    }

    /// <summary>Cancela a assinatura ativa (US-08). Acesso mantido ate o fim do periodo.</summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(CancellationToken ct)
    {
        var result = await sender.Send(new CancelSubscriptionCommand(User.GetUserId()), ct);
        return result.IsSuccess ? NoContent() : ApiResults.Problem(result.Error);
    }
}

public sealed record SubscribeRequest(string PlanCode, string PaymentMethod);
