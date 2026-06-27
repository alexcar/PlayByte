namespace PlayByte.Domain.Billing;

public readonly record struct SubscriptionId(Guid Value)
{
    public static SubscriptionId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}
