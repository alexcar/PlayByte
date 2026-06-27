using MediatR;
using Microsoft.AspNetCore.Mvc;
using PlayByte.Api.Extensions;
using PlayByte.Application.Billing.Payments.Commands.ApprovePayment;

namespace PlayByte.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Produces("application/json")]
public sealed class PaymentsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Confirma a aprovacao de um pagamento. Endpoint tipicamente chamado pelo WEBHOOK do
    /// gateway. E' idempotente: reentregas do mesmo pagamento nao renovam a assinatura de novo.
    ///
    /// ATENCAO DE SEGURANCA (producao): antes de processar, VALIDE a assinatura do provedor
    /// (ex.: header Stripe-Signature / HMAC do Mercado Pago) e, idealmente, deduplique pelo
    /// id do evento do gateway. Sem isso, o endpoint aceita chamadas forjadas.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(
        Guid id, [FromBody] ApprovePaymentRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new ApprovePaymentCommand(id, request.GatewayTransactionId), ct);

        return result.IsSuccess
            ? Ok()
            : ApiResults.Problem(result.Error);
    }
}

/// <summary>Payload de aprovacao de pagamento (do gateway).</summary>
public sealed record ApprovePaymentRequest(string GatewayTransactionId);
