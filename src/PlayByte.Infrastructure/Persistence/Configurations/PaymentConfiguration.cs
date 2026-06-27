using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayByte.Domain.Billing;
using PlayByte.Domain.Billing.Enumerations;
using PlayByte.Domain.Users;

namespace PlayByte.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => new PaymentId(value))
            .ValueGeneratedNever();

        // Referencia por ID ao agregado Subscription (agregados separados).
        builder.Property(p => p.SubscriptionId)
            .HasConversion(id => id.Value, value => new SubscriptionId(value))
            .IsRequired();

        builder.Property(p => p.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .IsRequired();

        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("amount").HasColumnType("numeric(19,4)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        });
        builder.Navigation(p => p.Amount).IsRequired();

        builder.Property(p => p.Method)
            .HasColumnName("method")
            .HasConversion(m => m.Id, id => PaymentMethod.FromId(id))
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion(s => s.Id, id => PaymentStatus.FromId(id))
            .IsRequired();

        builder.Property(p => p.GatewayTransactionId).HasMaxLength(200);
        builder.Property(p => p.ProcessedAtUtc);
        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc);

        builder.HasIndex(p => p.SubscriptionId);
        builder.HasIndex(p => p.UserId);

        builder.Ignore(p => p.DomainEvents);
    }
}
