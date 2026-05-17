using BunningsSizzlingHotProducts.Domain.Entities;

namespace BunningsSizzlingHotProducts.Application.Abstractions;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);
}
