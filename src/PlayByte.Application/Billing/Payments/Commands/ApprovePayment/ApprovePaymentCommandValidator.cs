using FluentValidation;

namespace PlayByte.Application.Billing.Payments.Commands.ApprovePayment;

public sealed class ApprovePaymentCommandValidator : AbstractValidator<ApprovePaymentCommand>
{
    public ApprovePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("O id do pagamento e obrigatorio.");

        RuleFor(x => x.GatewayTransactionId)
            .NotEmpty().WithMessage("O id da transacao do gateway e obrigatorio.")
            .MaximumLength(200);
    }
}
