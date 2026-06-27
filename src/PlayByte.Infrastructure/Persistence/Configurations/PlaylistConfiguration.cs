using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Playlists;
using PlayByte.Domain.Users;

namespace PlayByte.Infrastructure.Persistence.Configurations;

internal sealed class PlaylistConfiguration : IEntityTypeConfiguration<Playlist>
{
    public void Configure(EntityTypeBuilder<Playlist> builder)
    {
        builder.ToTable("playlists");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => new PlaylistId(value))
            .ValueGeneratedNever();

        builder.Property(p => p.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .IsRequired();

        builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc);

        builder.HasIndex(p => p.UserId);

        builder.HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(i => i.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(p => p.DomainEvents);
    }
}

internal sealed class PlaylistItemConfiguration : IEntityTypeConfiguration<PlaylistItem>
{
    public void Configure(EntityTypeBuilder<PlaylistItem> builder)
    {
        builder.ToTable("playlist_items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasConversion(id => id.Value, value => new PlaylistItemId(value))
            .ValueGeneratedNever();

        builder.Property(i => i.PlaylistId)
            .HasConversion(id => id.Value, value => new PlaylistId(value))
            .IsRequired();

        builder.Property(i => i.TrackId)
            .HasConversion(id => id.Value, value => new TrackId(value))
            .IsRequired();

        builder.Property(i => i.Position).IsRequired();
        builder.Property(i => i.AddedAtUtc).IsRequired();

        // Dedupe tambem no banco (US-13 c3): nao repete a mesma faixa na playlist.
        builder.HasIndex(i => new { i.PlaylistId, i.TrackId }).IsUnique();
    }
}
