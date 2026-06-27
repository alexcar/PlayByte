using PlayByte.Domain.Abstractions;

namespace PlayByte.Domain.Billing.Enumerations;

public sealed class PaymentMethod : Enumeration<PaymentMethod>
{
    public bool SupportsRecurring { get; }

    private PaymentMethod(int id, string name, bool supportsRecurring) : base(id, name) =>
        SupportsRecurring = supportsRecurring;

    public static readonly PaymentMethod CreditCard = new(1, nameof(CreditCard), true);
    public static readonly PaymentMethod Pix        = new(2, nameof(Pix),        false);
    public static readonly PaymentMethod Boleto     = new(3, nameof(Boleto),     false);
    public static readonly PaymentMethod DebitCard  = new(4, nameof(DebitCard),  true);

    public static readonly IReadOnlyList<PaymentMethod> All =
        [CreditCard, Pix, Boleto, DebitCard];

    public static PaymentMethod FromId(int id) => All.First(m => m.Id == id);
}
