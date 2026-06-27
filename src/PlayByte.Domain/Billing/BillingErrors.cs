using PlayByte.Domain.Common;

namespace PlayByte.Domain.Billing;

public static class SubscriptionErrors
{
    public static readonly Error CannotRenewInactive =
        Error.Conflict("Subscription.CannotRenewInactive", "Nao e possivel renovar uma assinatura cancelada ou expirada.");
    public static readonly Error InvalidTransition =
        Error.Conflict("Subscription.InvalidTransition", "A transicao de status nao e permitida.");
    public static readonly Error PeriodStillActive =
        Error.Conflict("Subscription.PeriodStillActive", "Nao e possivel expirar uma assinatura com vigencia ativa.");

    public static readonly Error NoTrialAvailable =
        Error.Validation("Subscription.NoTrialAvailable", "Este plano nao oferece periodo de trial.");
    public static readonly Error NoActiveSubscription =
        Error.NotFound("Subscription.NoActiveSubscription", "Nenhuma assinatura ativa encontrada para o usuario.");

    public static Error NotFound(SubscriptionId id) =>
        Error.NotFound("Subscription.NotFound", $"Assinatura '{id}' nao encontrada.");
}

public static class PaymentErrors
{
    public static readonly Error NonPositiveAmount =
        Error.Validation("Payment.NonPositiveAmount", "O valor do pagamento deve ser maior que zero.");
    public static readonly Error NotPending =
        Error.Conflict("Payment.NotPending", "Apenas um pagamento pendente pode ser processado.");
    public static readonly Error MissingGatewayId =
        Error.Validation("Payment.MissingGatewayId", "O id da transacao do gateway e obrigatorio para aprovar.");
    public static readonly Error OnlyApprovedCanBeRefunded =
        Error.Conflict("Payment.OnlyApprovedCanBeRefunded", "Apenas um pagamento aprovado pode ser estornado.");

    public static Error NotFound(PaymentId id) =>
        Error.NotFound("Payment.NotFound", $"Pagamento '{id}' nao encontrado.");
}
