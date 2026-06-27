using FluentValidation;

namespace PlayByte.Application.Billing.Subscriptions.Commands.SubscribeToPlan;

public sealed class SubscribeToPlanCommandValidator : AbstractValidator<SubscribeToPlanCommand>
{
    public SubscribeToPlanCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PlanCode).NotEmpty();
        RuleFor(x => x.PaymentMethod).NotEmpty();
    }
}
