using BunningsSizzlingHotProducts.Infrastructure.Persistence;
using BunningsSizzlingHotProducts.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace BunningsSizzlingHotProducts.Infrastructure.Seeding;

public sealed class DatabaseSeeder(SizzlingHotProductsDbContext db)
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task SeedAsync(string ordersJsonPath, string productsJsonPath, CancellationToken ct)
    {
        await db.Database.MigrateAsync(ct);

        if (await db.Products.AnyAsync(ct)) return;

        var products = JsonSerializer.Deserialize<SeedProduct[]>(
            await File.ReadAllTextAsync(productsJsonPath, ct), JsonOpts) ?? [];
        var seedOrders = JsonSerializer.Deserialize<SeedOrder[]>(
            await File.ReadAllTextAsync(ordersJsonPath, ct), JsonOpts) ?? [];

        db.Products.AddRange(products.Select(p => new ProductRow { Id = p.Id, Name = p.Name }));

        // Two-pass: capture completed-order dates so we can populate OriginalOrderDate on cancellations.
        var completedDates = seedOrders
            .Where(o => string.Equals(o.Status, "completed", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(o => o.OrderId, o => ParseDate(o.Date));

        foreach (var o in seedOrders)
        {
            var date = ParseDate(o.Date);
            var isCancelled = string.Equals(o.Status, "cancelled", StringComparison.OrdinalIgnoreCase);

            var row = new OrderRow
            {
                OrderId = o.OrderId,
                CustomerId = o.CustomerId,
                Date = date,
                Status = isCancelled ? "cancelled" : "completed",
                OriginalOrderDate = isCancelled && completedDates.TryGetValue(o.OrderId, out var d) ? d : null,
                Entries = (o.Entries ?? [])
                    .Select(e => new OrderEntryRow
                    {
                        OrderId = o.OrderId,
                        ProductId = e.Id,
                        Quantity = e.Quantity
                    })
                    .ToList()
            };

            // For cancellation rows we still want the original entries — load them from the matching completed row.
            if (isCancelled && row.Entries.Count == 0)
            {
                var matching = seedOrders.FirstOrDefault(
                    x => x.OrderId == o.OrderId
                      && string.Equals(x.Status, "completed", StringComparison.OrdinalIgnoreCase));
                if (matching?.Entries is not null)
                {
                    row.Entries.AddRange(matching.Entries.Select(e => new OrderEntryRow
                    {
                        OrderId = o.OrderId,
                        ProductId = e.Id,
                        Quantity = e.Quantity
                    }));
                }
            }

            db.Orders.Add(row);
        }

        await db.SaveChangesAsync(ct);
    }

    private static DateOnly ParseDate(string s) =>
        DateOnly.ParseExact(s, "dd/MM/yyyy", CultureInfo.InvariantCulture);
}
