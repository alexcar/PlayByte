using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayByte.Infrastructure.Logging;

namespace PlayByte.Infrastructure.Persistence.Configurations;

/// <summary>
/// O schema de error_logs e' gerenciado pelo EF (migrations), mas a ESCRITA
/// e' feita via Dapper (ErrorLogger), desacoplada do change tracker.
/// </summary>
internal sealed class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.ToTable("error_logs");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.OccurredAtUtc).IsRequired();
        builder.Property(e => e.Message).IsRequired();
        builder.Property(e => e.ExceptionType).HasMaxLength(500).IsRequired();
        builder.Property(e => e.StackTrace);
        builder.Property(e => e.Source).HasMaxLength(500);
        builder.Property(e => e.RequestPath).HasMaxLength(2000);
        builder.Property(e => e.RequestMethod).HasMaxLength(10);
        builder.Property(e => e.StatusCode);
        builder.Property(e => e.CorrelationId).HasMaxLength(100);

        builder.HasIndex(e => e.OccurredAtUtc);
        builder.HasIndex(e => e.CorrelationId);
    }
}
