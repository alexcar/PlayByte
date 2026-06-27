using PlayByte.Domain.Abstractions;

namespace PlayByte.Domain.Billing.Enumerations;

public sealed class SubscriptionStatus : Enumeration<SubscriptionStatus>
{
    public bool GrantsAccess { get; }

    private SubscriptionStatus(int id, string name, bool grantsAccess) : base(id, name) =>
        GrantsAccess = grantsAccess;

    public static readonly SubscriptionStatus PendingPayment = new(1, nameof(PendingPayment), false);
    public static readonly SubscriptionStatus Trialing       = new(2, nameof(Trialing),       true);
    public static readonly SubscriptionStatus Active         = new(3, nameof(Active),         true);
    public static readonly SubscriptionStatus PastDue        = new(4, nameof(PastDue),        true);
    public static readonly SubscriptionStatus Canceled       = new(5, nameof(Canceled),       true);
    public static readonly SubscriptionStatus Expired        = new(6, nameof(Expired),        false);

    public static readonly IReadOnlyList<SubscriptionStatus> All =
        [PendingPayment, Trialing, Active, PastDue, Canceled, Expired];

    public static SubscriptionStatus FromId(int id) => All.First(s => s.Id == id);
}
