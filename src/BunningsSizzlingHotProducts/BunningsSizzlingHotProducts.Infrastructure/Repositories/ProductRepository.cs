using BunningsSizzlingHotProducts.Application.Abstractions;
using BunningsSizzlingHotProducts.Domain.Entities;
using BunningsSizzlingHotProducts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BunningsSizzlingHotProducts.Infrastructure.Repositories;

public sealed class ProductRepository(SizzlingHotProductsDbContext db) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        var rows = await db.Products
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return rows.Select(r => new Product(r.Id, r.Name)).ToList();
    }
}
