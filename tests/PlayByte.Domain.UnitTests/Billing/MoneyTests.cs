using PlayByte.Domain.Billing.ValueObjects;
using Shouldly;
using Xunit;

namespace PlayByte.Domain.UnitTests.Billing;

public sealed class MoneyTests
{
    [Fact]
    public void Create_DeveArredondarParaDuasCasas()
    {
        var money = Money.Create(10.125m, "BRL").Value;

        money.Amount.ShouldBe(10.12m); // banker's rounding (ToEven)
        money.Currency.ShouldBe("BRL");
    }

    [Fact]
    public void Create_ComMoedaInvalida_DeveFalhar()
    {
        Money.Create(10m, "REAL").IsFailure.ShouldBeTrue();
        Money.Create(10m, "").IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Add_ComMoedasDiferentes_DeveLancar()
    {
        var brl = Money.Create(10m, "BRL").Value;
        var usd = Money.Create(10m, "USD").Value;

        Should.Throw<InvalidOperationException>(() => brl.Add(usd));
    }

    [Fact]
    public void Add_ComMesmaMoeda_DeveSomar()
    {
        var a = Money.Create(10m, "BRL").Value;
        var b = Money.Create(5.50m, "BRL").Value;

        a.Add(b).Amount.ShouldBe(15.50m);
    }
}
