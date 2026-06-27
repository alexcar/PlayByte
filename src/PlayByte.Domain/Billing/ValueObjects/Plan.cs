using PlayByte.Domain.Common;

namespace PlayByte.Domain.Billing.ValueObjects;

public enum BillingInterval { Monthly = 1, Annual = 2 }

/// <summary>
/// Snapshot imutavel dos termos aceitos pelo assinante. Mudancas no catalogo
/// nao afetam assinaturas existentes.
/// </summary>
public sealed record Plan
{
    public string Name { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public BillingInterval Interval { get; private set; }
    public int TrialDays { get; private set; }

    private Plan() { } // for EF Core

    private Plan(string name, Money price, BillingInterval interval, int trialDays) =>
        (Name, Price, Interval, TrialDays) = (name, price, interval, trialDays);

    public static Result<Plan> Create(string? name, Money price, BillingInterval interval, int trialDays = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            return PlanErrors.NameRequired;
        if (!price.IsPositive)
            return PlanErrors.NonPositivePrice;
        if (trialDays < 0)
            return PlanErrors.NegativeTrial;

        return new Plan(name.Trim(), price, interval, trialDays);
    }

    public Result<BillingPeriod> CalculatePeriod(DateOnly start)
    {
        var end = Interval switch
        {
            BillingInterval.Monthly => start.AddMonths(1),
            BillingInterval.Annual  => start.AddYears(1),
            _ => start
        };
        return BillingPeriod.Create(start, end);
    }
}

public static class PlanErrors
{
    public static readonly Error NameRequired =
        Error.Validation("Plan.NameRequired", "O nome do plano e obrigatorio.");
    public static readonly Error NonPositivePrice =
        Error.Validation("Plan.NonPositivePrice", "O preco do plano deve ser maior que zero.");
    public static readonly Error NegativeTrial =
        Error.Validation("Plan.NegativeTrial", "O periodo de trial nao pode ser negativo.");
}
