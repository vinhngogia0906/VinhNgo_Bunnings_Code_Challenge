using BunningsSizzlingHotProducts.Infrastructure.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace BunningsSizzlingHotProducts.Api.IntegrationTests.Infrastructure;

public sealed class PostgresWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ConnEnvVar = "ConnectionStrings__Postgres";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("sizzling")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Environment.SetEnvironmentVariable(ConnEnvVar, _postgres.GetConnectionString());

        using var scope = Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

        var inputsDir = Path.Combine(FindRepoRoot(), "inputs");
        await seeder.SeedAsync(
            ordersJsonPath: Path.Combine(inputsDir, "orders.json"),
            productsJsonPath: Path.Combine(inputsDir, "products.json"),
            ct: CancellationToken.None);
    }

    public new async Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable(ConnEnvVar, null);
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !dir.GetFiles("README.md").Any())
            dir = dir.Parent;
        return dir?.FullName
               ?? throw new InvalidOperationException("Could not locate repo root (README.md).");
    }
}
