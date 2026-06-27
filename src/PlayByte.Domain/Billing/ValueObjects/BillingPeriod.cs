using PlayByte.Domain.Common;

namespace PlayByte.Domain.Billing.ValueObjects;

/// <summary>Janela de vigencia meio-aberta [Start, End).</summary>
public sealed record BillingPeriod
{
    public DateOnly Start { get; }
    public DateOnly End { get; }

    private BillingPeriod(DateOnly start, DateOnly end) => (Start, End) = (start, end);

    public static Result<BillingPeriod> Create(DateOnly start, DateOnly end)
    {
        if (end <= start)
            return BillingPeriodErrors.EndNotAfterStart;
        return new BillingPeriod(start, end);
    }

    public bool Contains(DateOnly date) => date >= Start && date < End;
    public bool IsExpired(DateOnly reference) => reference >= End;
    public int DurationInDays => End.DayNumber - Start.DayNumber;
}

public static class BillingPeriodErrors
{
    public static readonly Error EndNotAfterStart =
        Error.Validation("BillingPeriod.EndNotAfterStart", "O fim da vigencia deve ser posterior ao inicio.");
}
