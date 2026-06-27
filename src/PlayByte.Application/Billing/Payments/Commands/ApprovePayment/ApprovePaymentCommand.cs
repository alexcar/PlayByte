using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Billing.Payments.Commands.ApprovePayment;

/// <summary>
/// Confirma a aprovacao de um pagamento (tipicamente disparado por webhook do gateway).
/// </summary>
public sealed record ApprovePaymentCommand(Guid PaymentId, string GatewayTransactionId) : ICommand;
