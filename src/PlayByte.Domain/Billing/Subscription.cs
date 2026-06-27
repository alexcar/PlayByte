using PlayByte.Domain.Abstractions;
using PlayByte.Domain.Billing.Enumerations;
using PlayByte.Domain.Billing.Events;
using PlayByte.Domain.Billing.ValueObjects;
using PlayByte.Domain.Common;
using PlayByte.Domain.Users;

namespace PlayByte.Domain.Billing;

public sealed class Subscription : AggregateRoot<SubscriptionId>, IAuditable
{
    private Subscription() { } // EF Core

    private Subscription(SubscriptionId id, UserId userId, Plan plan,
        BillingPeriod period, SubscriptionStatus status) : base(id)
    {
        UserId = userId;
        Plan = plan;
        CurrentPeriod = period;
        Status = status;
    }

    public UserId UserId { get; private set; }
    public Plan Plan { get; private set; } = default!;
    public BillingPeriod CurrentPeriod { get; private set; } = default!;
    public SubscriptionStatus Status { get; private set; } = default!;
    public DateTimeOffset? CanceledAtUtc { get; private set; }

    /// <summary>Ultimo pagamento que ativou/renovou; chave de idempotencia.</summary>
    public PaymentId? LastRenewalPaymentId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Cria a assinatura em PendingPayment: o usuario AINDA NAO tem acesso pago.
    /// O acesso e' liberado apenas quando o primeiro pagamento e' aprovado (US-05 c4).
    /// </summary>
    public static Result<Subscription> StartPendingPayment(UserId userId, Plan plan, DateOnly startDate)
    {
        var periodResult = plan.CalculatePeriod(startDate);
        if (periodResult.IsFailure)
            return periodResult.Error;

        var sub = new Subscription(SubscriptionId.New(), userId, plan, periodResult.Value, SubscriptionStatus.PendingPayment);
        sub.RaiseDomainEvent(new SubscriptionStarted(sub.Id, userId, plan.Name, sub.Status.Name));
        return sub;
    }

    /// <summary>Inicia uma assinatura em trial (acesso imediato sem cobranca).</summary>
    public static Result<Subscription> StartTrial(UserId userId, Plan plan, DateOnly startDate)
    {
        if (plan.TrialDays <= 0)
            return SubscriptionErrors.NoTrialAvailable;

        var period = BillingPeriod.Create(startDate, startDate.AddDays(plan.TrialDays));
        if (period.IsFailure)
            return period.Error;

        var sub = new Subscription(SubscriptionId.New(), userId, plan, period.Value, SubscriptionStatus.Trialing);
        sub.RaiseDomainEvent(new SubscriptionStarted(sub.Id, userId, plan.Name, sub.Status.Name));
        return sub;
    }

    /// <summary>
    /// Aplica um pagamento aprovado. Idempotente por <paramref name="paymentId"/>.
    /// 1o pagamento (PendingPayment): ativa e levanta SubscriptionContracted (boas-vindas, US-06).
    /// Pagamentos seguintes: estende a vigencia e levanta SubscriptionRenewed (US-07).
    /// </summary>
    public Result RegisterApprovedPayment(PaymentId paymentId, DateOnly today)
    {
        if (LastRenewalPaymentId == paymentId)
            return Result.Success(); // reentrega do mesmo pagamento

        if (Status == SubscriptionStatus.Expired || Status == SubscriptionStatus.Canceled)
            return SubscriptionErrors.CannotRenewInactive;

        var isFirstPayment = Status == SubscriptionStatus.PendingPayment;

        if (!isFirstPayment)
        {
            // Renovacao: empilha a partir do fim da vigencia atual (ou de hoje, se vencida).
            var start = CurrentPeriod.End > today ? CurrentPeriod.End : today;
            var newPeriod = Plan.CalculatePeriod(start);
            if (newPeriod.IsFailure)
                return newPeriod.Error;
            CurrentPeriod = newPeriod.Value;
        }

        Status = SubscriptionStatus.Active;
        LastRenewalPaymentId = paymentId;

        if (isFirstPayment)
            RaiseDomainEvent(new SubscriptionContracted(
                Id, UserId, Plan.Name, Plan.Price.Amount, Plan.Price.Currency, CurrentPeriod.End, paymentId));
        else
            RaiseDomainEvent(new SubscriptionRenewed(
                Id, UserId, Plan.Name, Plan.Price.Amount, Plan.Price.Currency, CurrentPeriod.End, paymentId));

        return Result.Success();
    }

    public Result MarkPastDue()
    {
        if (Status != SubscriptionStatus.Active && Status != SubscriptionStatus.Trialing)
            return SubscriptionErrors.InvalidTransition;

        Status = SubscriptionStatus.PastDue;
        RaiseDomainEvent(new SubscriptionPastDue(Id, UserId));
        return Result.Success();
    }

    public Result Cancel(DateTimeOffset nowUtc)
    {
        if (Status == SubscriptionStatus.Canceled || Status == SubscriptionStatus.Expired)
            return Result.Success(); // idempotente

        Status = SubscriptionStatus.Canceled;
        CanceledAtUtc = nowUtc;
        RaiseDomainEvent(new SubscriptionCanceled(Id, UserId, Plan.Name, CurrentPeriod.End));
        return Result.Success();
    }

    public Result Expire(DateOnly today)
    {
        if (Status == SubscriptionStatus.Expired) return Result.Success();
        if (!CurrentPeriod.IsExpired(today)) return SubscriptionErrors.PeriodStillActive;

        Status = SubscriptionStatus.Expired;
        RaiseDomainEvent(new SubscriptionExpired(Id, UserId));
        return Result.Success();
    }

    /// <summary>Acesso pago efetivo: status concede acesso E a vigencia nao expirou.</summary>
    public bool HasActiveAccess(DateOnly today) =>
        Status.GrantsAccess && !CurrentPeriod.IsExpired(today);
}
