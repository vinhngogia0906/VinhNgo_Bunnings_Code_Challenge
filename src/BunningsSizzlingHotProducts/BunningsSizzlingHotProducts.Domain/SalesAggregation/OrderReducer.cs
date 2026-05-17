using BunningsSizzlingHotProducts.Domain.Entities;
using BunningsSizzlingHotProducts.Domain.Enums;

namespace BunningsSizzlingHotProducts.Domain.SalesAggregation;

public sealed class OrderReducer
{
    public IReadOnlyList<Order> Reduce(IEnumerable<Order> rawOrders)
    {
        var byOrderId = new Dictionary<string, Order>(StringComparer.Ordinal);
        var cancelled = new HashSet<string>(StringComparer.Ordinal);

        foreach (var order in rawOrders)
        {
            if (order.Status == OrderStatus.Cancelled)
            {
                cancelled.Add(order.OrderId);
            }
            else
            {
                // Last completed write wins — guards against duplicate completed rows
                byOrderId[order.OrderId] = order;
            }
        }

        return byOrderId
            .Where(kv => !cancelled.Contains(kv.Key))
            .Select(kv => kv.Value)
            .ToList();
    }
}