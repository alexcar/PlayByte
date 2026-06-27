using PlayByte.Domain.Abstractions;

namespace PlayByte.Domain.Billing.Enumerations;

public sealed class PaymentStatus : Enumeration<PaymentStatus>
{
    private PaymentStatus(int id, string name) : base(id, name) { }

    public static readonly PaymentStatus Pending  = new(1, nameof(Pending));
    public static readonly PaymentStatus Approved = new(2, nameof(Approved));
    public static readonly PaymentStatus Declined = new(3, nameof(Declined));
    public static readonly PaymentStatus Refunded = new(4, nameof(Refunded));

    public static readonly IReadOnlyList<PaymentStatus> All =
        [Pending, Approved, Declined, Refunded];

    public static PaymentStatus FromId(int id) => All.First(s => s.Id == id);
}
