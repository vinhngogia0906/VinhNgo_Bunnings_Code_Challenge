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

        var rawProducts = await File.ReadAllTextAsync(productsJsonPath, ct);
        var products = ParseJsonTolerant<SeedProduct[]>(rawProducts, JsonOpts) ?? [];
        var rawOrders = await File.ReadAllTextAsync(ordersJsonPath, ct);
        var seedOrders = ParseJsonTolerant<SeedOrder[]>(rawOrders, JsonOpts) ?? [];

        db.Products.AddRange(products.Select(p => new ProductRow { Id = p.Id, Name = p.Name }));

        // Pass 1 — one OrderRow per OrderId, populated from the completed seed record.
        var ordersById = new Dictionary<string, OrderRow>(StringComparer.Ordinal);
        foreach (var o in seedOrders.Where(o => NormaliseStatus(o) == "completed"))
        {
            if (ordersById.ContainsKey(o.OrderId))
                throw new InvalidOperationException(
                    $"Duplicate completed order in seed data: {o.OrderId}");

            ordersById[o.OrderId] = new OrderRow
            {
                OrderId = o.OrderId,
                CustomerId = o.CustomerId,
                Date = ParseDate(o.Date),
                Status = "completed",
                OriginalOrderDate = null,
                Entries = (o.Entries ?? [])
                    .Select(e => new OrderEntryRow
                    {
                        OrderId = o.OrderId,
                        ProductId = e.Id,
                        Quantity = e.Quantity,
                    })
                    .ToList(),
            };
        }

        // Pass 2 — cancellations mutate the existing row instead of inserting a new one.
        foreach (var o in seedOrders.Where(o => NormaliseStatus(o) == "cancelled"))
        {
            if (!ordersById.TryGetValue(o.OrderId, out var row))
                throw new InvalidOperationException(
                    $"Cancellation references unknown order: {o.OrderId}");

            row.OriginalOrderDate = row.Date;
            row.Date = ParseDate(o.Date);
            row.Status = "cancelled";
        }

        db.Orders.AddRange(ordersById.Values);
        await db.SaveChangesAsync(ct);
    }

    private static DateOnly ParseDate(string s) =>
        DateOnly.ParseExact(s, "dd/MM/yyyy", CultureInfo.InvariantCulture);

    // Use this to clean dirty JSON inputs that have raw CR/LF line breaks inside string literals (a common artifact of line-wrapping in source files).
    static T? ParseJsonTolerant<T>(string raw, JsonSerializerOptions opts)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(raw, opts);
        }
        catch (JsonException)
        {
            // The provided inputs have raw CR/LF inserted inside string literals
            // (a line-wrap artifact, not legitimate data). Strip both CR and LF
            // so the fallback survives git's text=auto normalisation across
            // platforms — Windows checkouts see \r\n inside strings; Linux/CI
            // checkouts see bare \n after normalisation. Stripping either is
            // safe: between JSON tokens they are optional whitespace; inside
            // strings they are the corruption we are repairing.
            var normalized = raw.Replace("\r", "").Replace("\n", "");
            return JsonSerializer.Deserialize<T>(normalized, opts);
        }
    }

    private static string NormaliseStatus(SeedOrder o) =>
      o.Status?.Trim().ToLowerInvariant() switch
      {
          "completed" => "completed",
          "cancelled" => "cancelled",
          _ => throw new InvalidOperationException(
              $"Unknown order status '{o.Status}' on order {o.OrderId}"),
      };
}
