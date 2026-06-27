using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Favorites;
using PlayByte.Domain.Users;

namespace PlayByte.Infrastructure.Persistence.Configurations;

internal sealed class FavoriteTrackConfiguration : IEntityTypeConfiguration<FavoriteTrack>
{
    public void Configure(EntityTypeBuilder<FavoriteTrack> builder)
    {
        builder.ToTable("favorite_tracks");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasConversion(id => id.Value, value => new FavoriteTrackId(value))
            .ValueGeneratedNever();

        builder.Property(f => f.UserId)
            .HasConversion(id => id.Value, value => new UserId(value)).IsRequired();
        builder.Property(f => f.TrackId)
            .HasConversion(id => id.Value, value => new TrackId(value)).IsRequired();

        builder.Property(f => f.CreatedAtUtc).IsRequired();
        builder.Property(f => f.UpdatedAtUtc);

        builder.HasIndex(f => new { f.UserId, f.TrackId }).IsUnique();
        builder.Ignore(f => f.DomainEvents);
    }
}

internal sealed class FavoriteBandConfiguration : IEntityTypeConfiguration<FavoriteBand>
{
    public void Configure(EntityTypeBuilder<FavoriteBand> builder)
    {
        builder.ToTable("favorite_bands");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasConversion(id => id.Value, value => new FavoriteBandId(value))
            .ValueGeneratedNever();

        builder.Property(f => f.UserId)
            .HasConversion(id => id.Value, value => new UserId(value)).IsRequired();
        builder.Property(f => f.BandId)
            .HasConversion(id => id.Value, value => new BandId(value)).IsRequired();

        builder.Property(f => f.CreatedAtUtc).IsRequired();
        builder.Property(f => f.UpdatedAtUtc);

        builder.HasIndex(f => new { f.UserId, f.BandId }).IsUnique();
        builder.Ignore(f => f.DomainEvents);
    }
}
