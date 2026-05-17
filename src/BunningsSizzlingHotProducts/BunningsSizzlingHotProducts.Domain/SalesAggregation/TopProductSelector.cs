using BunningsSizzlingHotProducts.Domain.Entities;

namespace BunningsSizzlingHotProducts.Domain.SalesAggregation;

public sealed class TopProductSelector
{
    public string? Select(
        IEnumerable<ProductDailySale> sales,
        IEnumerable<Product> products)
    {
        var nameById = products.ToDictionary(p => p.Id, p => p.Name, StringComparer.Ordinal);

        var byProduct = sales
            .GroupBy(s => s.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Total = g.Sum(s => s.Sales),
                Name = nameById.GetValueOrDefault(g.Key, g.Key)
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Name, StringComparer.Ordinal)
            .FirstOrDefault();

        return byProduct?.Name;
    }
}
