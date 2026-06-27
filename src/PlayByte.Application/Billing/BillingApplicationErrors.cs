using PlayByte.Domain.Common;

namespace PlayByte.Application.Billing;

public static class BillingApplicationErrors
{
    public static readonly Error PlanNotFound =
        Error.NotFound("Billing.PlanNotFound", "Plano nao encontrado no catalogo.");
    public static readonly Error InvalidPaymentMethod =
        Error.Validation("Billing.InvalidPaymentMethod", "Metodo de pagamento invalido.");
}
