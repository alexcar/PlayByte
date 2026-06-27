using PlayByte.Domain.Billing;
using PlayByte.Domain.Billing.Enumerations;
using PlayByte.Domain.Billing.Events;
using PlayByte.Domain.Billing.ValueObjects;
using PlayByte.Domain.Users;
using Shouldly;
using Xunit;

namespace PlayByte.Domain.UnitTests.Billing;

public sealed class SubscriptionTests
{
    private static Plan PlanoMensal(int trialDays = 0)
    {
        var price = Money.Create(19.90m, "BRL").Value;
        return Plan.Create("Premium", price, BillingInterval.Monthly, trialDays).Value;
    }

    [Fact]
    public void StartPendingPayment_DeveFicarPendenteSemAcesso()
    {
        var sub = Subscription.StartPendingPayment(UserId.New(), PlanoMensal(), new DateOnly(2026, 1, 1)).Value;

        sub.Status.ShouldBe(SubscriptionStatus.PendingPayment);
        sub.HasActiveAccess(new DateOnly(2026, 1, 10)).ShouldBeFalse(); // sem pagamento, sem acesso (US-05 c4)
        sub.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<SubscriptionStarted>();
    }

    [Fact]
    public void StartTrial_DeveFicarEmTrialingComAcesso()
    {
        var sub = Subscription.StartTrial(UserId.New(), PlanoMensal(trialDays: 7), new DateOnly(2026, 1, 1)).Value;

        sub.Status.ShouldBe(SubscriptionStatus.Trialing);
        sub.CurrentPeriod.DurationInDays.ShouldBe(7);
        sub.HasActiveAccess(new DateOnly(2026, 1, 5)).ShouldBeTrue();
    }

    [Fact]
    public void RegisterApprovedPayment_PrimeiroPagamento_DeveAtivarELevantarContracted()
    {
        var sub = Subscription.StartPendingPayment(UserId.New(), PlanoMensal(), new DateOnly(2026, 1, 1)).Value;
        sub.ClearDomainEvents(); // descarta o SubscriptionStarted para isolar a asercao

        var result = sub.RegisterApprovedPayment(PaymentId.New(), new DateOnly(2026, 1, 1));

        result.IsSuccess.ShouldBeTrue();
        sub.Status.ShouldBe(SubscriptionStatus.Active);
        sub.CurrentPeriod.End.ShouldBe(new DateOnly(2026, 2, 1)); // 1o pagamento confirma, nao estende
        sub.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<SubscriptionContracted>();
    }

    [Fact]
    public void RegisterApprovedPayment_MesmoPagamento_DeveSerIdempotente()
    {
        var sub = Subscription.StartPendingPayment(UserId.New(), PlanoMensal(), new DateOnly(2026, 1, 1)).Value;
        var paymentId = PaymentId.New();

        sub.RegisterApprovedPayment(paymentId, new DateOnly(2026, 1, 1));
        var fim = sub.CurrentPeriod.End;

        // Reentrega do MESMO pagamento (retry do Outbox): nao estende.
        var segunda = sub.RegisterApprovedPayment(paymentId, new DateOnly(2026, 1, 15));

        segunda.IsSuccess.ShouldBeTrue();
        sub.CurrentPeriod.End.ShouldBe(fim);
        sub.LastRenewalPaymentId.ShouldBe(paymentId);
    }

    [Fact]
    public void RegisterApprovedPayment_PagamentosDiferentes_DeveEstenderELevantarRenewed()
    {
        var sub = Subscription.StartPendingPayment(UserId.New(), PlanoMensal(), new DateOnly(2026, 1, 1)).Value;

        sub.RegisterApprovedPayment(PaymentId.New(), new DateOnly(2026, 1, 1));  // contrata, fim 2026-02-01
        sub.ClearDomainEvents();
        sub.RegisterApprovedPayment(PaymentId.New(), new DateOnly(2026, 1, 20)); // renova -> fim 2026-03-01

        sub.CurrentPeriod.End.ShouldBe(new DateOnly(2026, 3, 1));
        sub.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<SubscriptionRenewed>();
    }

    [Fact]
    public void HasActiveAccess_CanceladaDentroDaVigencia_DeveManterAcesso()
    {
        var sub = Subscription.StartPendingPayment(UserId.New(), PlanoMensal(), new DateOnly(2026, 1, 1)).Value;
        sub.RegisterApprovedPayment(PaymentId.New(), new DateOnly(2026, 1, 1)); // ativa ate 2026-02-01
        sub.Cancel(DateTimeOffset.UtcNow);

        sub.HasActiveAccess(new DateOnly(2026, 1, 15)).ShouldBeTrue();  // US-08 c3
        sub.HasActiveAccess(new DateOnly(2026, 2, 2)).ShouldBeFalse();
    }

    [Fact]
    public void RegisterApprovedPayment_AposCancelar_DeveFalhar()
    {
        var sub = Subscription.StartPendingPayment(UserId.New(), PlanoMensal(), new DateOnly(2026, 1, 1)).Value;
        sub.RegisterApprovedPayment(PaymentId.New(), new DateOnly(2026, 1, 1));
        sub.Cancel(DateTimeOffset.UtcNow);

        var result = sub.RegisterApprovedPayment(PaymentId.New(), new DateOnly(2026, 2, 1));

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SubscriptionErrors.CannotRenewInactive);
    }
}
