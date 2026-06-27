using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using PlayByte.Infrastructure.Persistence;
using Testcontainers.MsSql;
using Xunit;

namespace PlayByte.Api.IntegrationTests;

/// <summary>
/// Sobe a API completa em memoria (WebApplicationFactory) apontando para um SQL Server
/// real efemero (Testcontainers). O schema e' criado via EnsureCreated a partir do modelo
/// do EF Core - assim EF (escrita) e Dapper (leitura) rodam contra o banco de verdade.
/// </summary>
public sealed class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _database = new MsSqlBuilder()
        // Reutiliza a imagem ja disponivel localmente (evita download).
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();

        // IMPORTANTE: no minimal hosting, o Program le a connection string (em AddInfrastructure)
        // ANTES do Build(); por isso, o override via ConfigureAppConfiguration do
        // WebApplicationFactory chegaria tarde demais. A variavel de ambiente, por outro lado,
        // e' lida pelo WebApplication.CreateBuilder (AddEnvironmentVariables) ja na criacao do
        // builder. "__" vira ":" -> ConnectionStrings:Database.
        Environment.SetEnvironmentVariable("ConnectionStrings__Database", _database.GetConnectionString());

        // Acessar Services dispara o build do host (ja com a connection string do container).
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Resiliencia ao cold start: aguarda o SQL Server aceitar conexoes antes de criar o schema.
        await WaitUntilAvailableAsync(context);
        await context.Database.EnsureCreatedAsync();
    }

    private async Task WaitUntilAvailableAsync(ApplicationDbContext context)
    {
        var deadline = DateTime.UtcNow.AddSeconds(90);
        while (true)
        {
            try
            {
                if (await context.Database.CanConnectAsync())
                    return;
            }
            catch when (DateTime.UtcNow < deadline)
            {
                // ainda subindo: tenta de novo ate o deadline
            }

            if (DateTime.UtcNow >= deadline)
                throw new InvalidOperationException(
                    $"SQL Server nao ficou disponivel a tempo. ConnectionString='{_database.GetConnectionString()}'");

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }

    public override async ValueTask DisposeAsync()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Database", null);
        await _database.DisposeAsync();
        await base.DisposeAsync();
    }
}

/// <summary>Compartilha um unico container/factory entre todas as classes de teste de integracao.</summary>
[CollectionDefinition(Name)]
public sealed class IntegrationCollection : ICollectionFixture<IntegrationTestFactory>
{
    public const string Name = "Integration";
}
