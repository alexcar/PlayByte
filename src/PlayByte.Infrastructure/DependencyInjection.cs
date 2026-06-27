using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlayByte.Application.Abstractions.Data;
using PlayByte.Application.Abstractions.Logging;
using PlayByte.Application.Abstractions.Messaging;
using PlayByte.Application.Abstractions.Notifications;
using PlayByte.Application.Abstractions.Security;
using PlayByte.Domain.Billing;
using PlayByte.Domain.Catalog;
using PlayByte.Domain.Favorites;
using PlayByte.Domain.Playlists;
using PlayByte.Domain.Users;
using PlayByte.Infrastructure.Authentication;
using PlayByte.Infrastructure.Logging;
using PlayByte.Infrastructure.Notifications;
using PlayByte.Infrastructure.Persistence;
using PlayByte.Infrastructure.Persistence.Interceptors;
using PlayByte.Infrastructure.Persistence.Repositories;
using PlayByte.Infrastructure.Security;

namespace PlayByte.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' nao configurada.");

        // Fonte de tempo testavel.
        services.AddSingleton(TimeProvider.System);

        // Options.
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Interceptors: auditoria, soft delete e despacho de eventos de dominio (pos-commit).
        services.AddScoped<AuditingInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options
                .UseSqlServer(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(
                    sp.GetRequiredService<AuditingInterceptor>(),
                    sp.GetRequiredService<SoftDeleteInterceptor>(),
                    sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        // O DbContext do EF Core ja e' o Unit of Work (SaveChanges numa unica transacao).
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Dapper (lado de leitura) compartilha a mesma connection string.
        services.AddSingleton<ISqlConnectionFactory>(_ => new SqlConnectionFactory(connectionString));

        // Dapper: SQL Server nao aceita DateOnly como parametro/coluna nativamente -> type handler.
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        // Despacho de eventos de dominio em processo (sem MediatR no Domain).
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Repositorios (lado de escrita).
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IBandRepository, BandRepository>();
        services.AddScoped<IPlaylistRepository, PlaylistRepository>();
        services.AddScoped<IFavoriteTrackRepository, FavoriteTrackRepository>();
        services.AddScoped<IFavoriteBandRepository, FavoriteBandRepository>();

        // Seguranca, e-mail e logging.
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();
        services.AddScoped<IExceptionLogger, ErrorLogger>();

        return services;
    }
}
