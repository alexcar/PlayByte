using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PlayByte.Infrastructure.Persistence;

/// <summary>Permite rodar 'dotnet ef migrations' sem subir a API.</summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("PLAYBYTE_DESIGN_CONNECTION")
            ?? "Server=localhost,1433;Database=playbyte;User Id=sa;Password=Playbyte_dev_pwd1;TrustServerCertificate=True;Encrypt=False";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new ApplicationDbContext(options);
    }
}
