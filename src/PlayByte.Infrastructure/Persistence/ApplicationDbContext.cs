using Microsoft.EntityFrameworkCore;
using PlayByte.Application.Abstractions.Data;
using PlayByte.Domain.Billing;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Favorites;
using PlayByte.Domain.Playlists;
using PlayByte.Domain.Users;
using PlayByte.Infrastructure.Logging;

namespace PlayByte.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Band> Bands => Set<Band>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<FavoriteTrack> FavoriteTracks => Set<FavoriteTrack>();
    public DbSet<FavoriteBand> FavoriteBands => Set<FavoriteBand>();
    public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
