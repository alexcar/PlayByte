using Microsoft.EntityFrameworkCore;
using PlayByte.Domain.Billing;

namespace PlayByte.Infrastructure.Persistence.Repositories;

internal sealed class PaymentRepository(ApplicationDbContext context) : IPaymentRepository
{
    public async Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken ct = default)
        => await context.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);

    public void Add(Payment payment) => context.Payments.Add(payment);
}
