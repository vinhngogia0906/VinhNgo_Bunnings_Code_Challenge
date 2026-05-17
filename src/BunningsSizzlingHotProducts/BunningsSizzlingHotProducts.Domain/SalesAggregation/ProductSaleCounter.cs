using BunningsSizzlingHotProducts.Domain.Entities;
using BunningsSizzlingHotProducts.Domain.Enums;

namespace BunningsSizzlingHotProducts.Domain.SalesAggregation;

public sealed record ProductDailySale(string ProductId, DateOnly Date, int Sales);

public sealed class ProductSaleCounter
{
    public IReadOnlyList<ProductDailySale> Count(IEnumerable<Order> orders)
    {
        // Rules 1 + 2: a (customer, product, date) tuple counts at most once.
        var uniquePurchases = new HashSet<(string CustomerId, string ProductId, DateOnly Date)>();

        foreach (var order in orders.Where(o => o.Status == OrderStatus.Completed))
        {
            foreach (var entry in order.Entries)
            {
                uniquePurchases.Add((order.CustomerId, entry.ProductId, order.Date));
            }
        }

        return uniquePurchases
            .GroupBy(t => (t.ProductId, t.Date))
            .Select(g => new ProductDailySale(g.Key.ProductId, g.Key.Date, g.Count()))
            .ToList();
    }
}
