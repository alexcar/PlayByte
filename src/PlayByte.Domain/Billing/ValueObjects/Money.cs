using PlayByte.Domain.Common;

namespace PlayByte.Domain.Billing.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; } // ISO 4217, ex.: "BRL"

    private Money(decimal amount, string currency) => (Amount, Currency) = (amount, currency);

    public static Result<Money> Create(decimal amount, string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return MoneyErrors.CurrencyRequired;

        var normalized = currency.Trim().ToUpperInvariant();
        if (normalized.Length != 3 || !normalized.All(char.IsLetter))
            return MoneyErrors.InvalidCurrency;

        return new Money(Math.Round(amount, 2, MidpointRounding.ToEven), normalized);
    }

    public static Money Zero(string currency) => Create(0m, currency).Value;

    public Money Add(Money other)      { EnsureSameCurrency(other); return new(Amount + other.Amount, Currency); }
    public Money Subtract(Money other) { EnsureSameCurrency(other); return new(Amount - other.Amount, Currency); }
    public Money Multiply(int factor)  => new(Amount * factor, Currency);

    public bool IsZero => Amount == 0m;
    public bool IsPositive => Amount > 0m;

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Operacao invalida entre moedas distintas: {Currency} e {other.Currency}.");
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}

public static class MoneyErrors
{
    public static readonly Error CurrencyRequired =
        Error.Validation("Money.CurrencyRequired", "A moeda e obrigatoria.");
    public static readonly Error InvalidCurrency =
        Error.Validation("Money.InvalidCurrency", "A moeda deve ser um codigo ISO 4217 de 3 letras.");
}
