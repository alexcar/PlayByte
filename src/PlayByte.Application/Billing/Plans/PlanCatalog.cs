using PlayByte.Domain.Billing.ValueObjects;

namespace PlayByte.Application.Billing.Plans;

public sealed record PlanDefinition(
    string Code, string Name, decimal Price, string Currency, BillingInterval Interval, int TrialDays);

/// <summary>Catalogo de planos pagos. O servidor e' a fonte da verdade dos precos.</summary>
public static class PlanCatalog
{
    public static readonly IReadOnlyList<PlanDefinition> Plans =
    [
        new("premium-monthly", "Premium Mensal", 19.90m, "BRL", BillingInterval.Monthly, 0),
        new("premium-annual",  "Premium Anual",  199.00m, "BRL", BillingInterval.Annual,  0),
    ];

    public static PlanDefinition? Find(string code) =>
        Plans.FirstOrDefault(p => string.Equals(p.Code, code, StringComparison.OrdinalIgnoreCase));
}
