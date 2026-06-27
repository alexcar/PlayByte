using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayByte.Domain.Users;
using PlayByte.Domain.Users.ValueObjects;

namespace PlayByte.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => new UserId(value))
            .ValueGeneratedNever();

        builder.Property(u => u.Name)
            .HasConversion(n => n.Value, s => UserName.Create(s).Value)
            .HasMaxLength(UserName.MaxLength)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasConversion(e => e.Value, s => Email.Create(s).Value)
            .HasMaxLength(Email.MaxLength)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasConversion(p => p.Value, s => PasswordHash.Create(s).Value)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.CreatedAtUtc).IsRequired();
        builder.Property(u => u.UpdatedAtUtc);
        builder.Property(u => u.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(u => u.DeletedAtUtc);

        // Indice unico de e-mail: a GARANTIA real de unicidade (filtra soft-deletados).
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("is_deleted = 0");

        // Global query filter: esconde registros soft-deletados em TODA consulta via EF.
        builder.HasQueryFilter(u => !u.IsDeleted);

        // Eventos de dominio nao sao persistidos.
        builder.Ignore(u => u.DomainEvents);
    }
}
