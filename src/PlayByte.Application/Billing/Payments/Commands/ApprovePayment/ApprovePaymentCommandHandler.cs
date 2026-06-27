using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Domain.Billing;
using PlayByte.Domain.Common;

namespace PlayByte.Application.Billing.Payments.Commands.ApprovePayment;

/// <summary>
/// Idempotencia de webhook: o agregado Payment.Approve() so transiciona (e so levanta
/// PaymentApproved) na PRIMEIRA vez. Reentregas do mesmo pagamento ja aprovado retornam
/// sucesso sem novo evento - logo, sem nova renovacao.
/// </summary>
internal sealed class ApprovePaymentCommandHandler(
    IPaymentRepository payments,
    IApplicationDbContext dbContext,
    TimeProvider timeProvider)
    : ICommandHandler<ApprovePaymentCommand>
{
    public async Task<Result> Handle(ApprovePaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(new PaymentId(command.PaymentId), cancellationToken);
        if (payment is null)
            return PaymentErrors.NotFound(new PaymentId(command.PaymentId));

        var result = payment.Approve(command.GatewayTransactionId, timeProvider.GetUtcNow());
        if (result.IsFailure)
            return result.Error;

        // Persiste o Payment; o SaveChanges despacha PaymentApproved, que o handler
        // de evento consome para renovar a assinatura (transacao separada).
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
