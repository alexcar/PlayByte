using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayByte.Domain.Catalog;

namespace PlayByte.Infrastructure.Persistence.Configurations;

internal sealed class BandConfiguration : IEntityTypeConfiguration<Band>
{
    public void Configure(EntityTypeBuilder<Band> builder)
    {
        builder.ToTable("bands");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasConversion(id => id.Value, value => new BandId(value))
            .ValueGeneratedNever();

        builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
        builder.Property(b => b.CoverImageUrl).HasMaxLength(1000);
        builder.Property(b => b.CreatedAtUtc).IsRequired();
        builder.Property(b => b.UpdatedAtUtc);

        builder.HasIndex(b => b.Name);

        builder.HasMany(b => b.Albums)
            .WithOne()
            .HasForeignKey(a => a.BandId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(b => b.Albums)
            .HasField("_albums")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(b => b.DomainEvents);
    }
}

internal sealed class AlbumConfiguration : IEntityTypeConfiguration<Album>
{
    public void Configure(EntityTypeBuilder<Album> builder)
    {
        builder.ToTable("albums");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => new AlbumId(value))
            .ValueGeneratedNever();

        builder.Property(a => a.BandId)
            .HasConversion(id => id.Value, value => new BandId(value))
            .IsRequired();

        builder.Property(a => a.Title).HasMaxLength(300).IsRequired();
        builder.Property(a => a.ReleaseYear).IsRequired();

        builder.HasMany(a => a.Tracks)
            .WithOne()
            .HasForeignKey(t => t.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.Tracks)
            .HasField("_tracks")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.ToTable("tracks");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => new TrackId(value))
            .ValueGeneratedNever();

        builder.Property(t => t.AlbumId)
            .HasConversion(id => id.Value, value => new AlbumId(value))
            .IsRequired();

        builder.Property(t => t.Title).HasMaxLength(300).IsRequired();
        builder.Property(t => t.DurationSeconds).IsRequired();

        builder.HasIndex(t => t.Title);
    }
}
