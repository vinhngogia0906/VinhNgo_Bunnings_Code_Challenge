using BunningsSizzlingHotProducts.Application.Abstractions;
using BunningsSizzlingHotProducts.Domain.Entities;
using BunningsSizzlingHotProducts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BunningsSizzlingHotProducts.Infrastructure.Repositories;

public sealed class ProductRepository(IDbContextFactory<SizzlingHotProductsDbContext> factory) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var rows = await db.Products
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return rows.Select(r => new Product(r.Id, r.Name)).ToList();
    }
}
