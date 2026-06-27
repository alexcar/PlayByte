using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayByte.Domain.Billing;
using PlayByte.Domain.Billing.Enumerations;
using PlayByte.Domain.Users;

namespace PlayByte.Infrastructure.Persistence.Configurations;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new SubscriptionId(value))
            .ValueGeneratedNever();

        builder.Property(s => s.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .IsRequired();

        // Plan = snapshot owned; Money aninhado como owned dentro do Plan.
        builder.OwnsOne(s => s.Plan, plan =>
        {
            plan.Property(p => p.Name).HasColumnName("plan_name").HasMaxLength(100).IsRequired();
            plan.Property(p => p.Interval).HasColumnName("plan_interval").HasConversion<int>().IsRequired();
            plan.Property(p => p.TrialDays).HasColumnName("plan_trial_days").IsRequired();

            plan.OwnsOne(p => p.Price, money =>
            {
                money.Property(m => m.Amount).HasColumnName("plan_price_amount").HasColumnType("numeric(19,4)").IsRequired();
                money.Property(m => m.Currency).HasColumnName("plan_price_currency").HasMaxLength(3).IsRequired();
            });
        });
        builder.Navigation(s => s.Plan).IsRequired();

        // Vigencia atual owned.
        builder.OwnsOne(s => s.CurrentPeriod, period =>
        {
            period.Property(p => p.Start).HasColumnName("current_period_start").IsRequired();
            period.Property(p => p.End).HasColumnName("current_period_end").IsRequired();
        });
        builder.Navigation(s => s.CurrentPeriod).IsRequired();

        // Enumeration class -> int.
        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasConversion(st => st.Id, id => SubscriptionStatus.FromId(id))
            .IsRequired();

        builder.Property(s => s.CanceledAtUtc);

        // Chave de idempotencia da renovacao (uuid nullable).
        builder.Property(s => s.LastRenewalPaymentId)
            .HasColumnName("last_renewal_payment_id")
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value.Value,
                value => value == null ? (PaymentId?)null : new PaymentId(value.Value));

        builder.Property(s => s.CreatedAtUtc).IsRequired();
        builder.Property(s => s.UpdatedAtUtc);

        builder.HasIndex(s => s.UserId);

        builder.Ignore(s => s.DomainEvents);
    }
}
