using BunningsSizzlingHotProducts.Application.Abstractions;
using BunningsSizzlingHotProducts.Infrastructure.Persistence;
using BunningsSizzlingHotProducts.Infrastructure.Repositories;
using BunningsSizzlingHotProducts.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BunningsSizzlingHotProducts.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("Postgres")
                   ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres");

        services.AddDbContextFactory<SizzlingHotProductsDbContext>(opt => opt.UseNpgsql(conn));
        services.AddScoped<SizzlingHotProductsDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<SizzlingHotProductsDbContext>>()
        .CreateDbContext());
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
