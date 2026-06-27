using PlayByte.Domain.Users;

namespace PlayByte.Domain.Billing;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(SubscriptionId id, CancellationToken ct = default);
    Task<Subscription?> GetActiveByUserIdAsync(UserId userId, CancellationToken ct = default);
    void Add(Subscription subscription);
}

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken ct = default);
    void Add(Payment payment);
}
