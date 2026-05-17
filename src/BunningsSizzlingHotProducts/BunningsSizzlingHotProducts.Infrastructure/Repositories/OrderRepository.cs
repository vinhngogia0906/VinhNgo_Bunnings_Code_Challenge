using BunningsSizzlingHotProducts.Application.Abstractions;
using BunningsSizzlingHotProducts.Domain.Entities;
using BunningsSizzlingHotProducts.Domain.Enums;
using BunningsSizzlingHotProducts.Infrastructure.Persistence;
using BunningsSizzlingHotProducts.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace BunningsSizzlingHotProducts.Infrastructure.Repositories;

public sealed class OrderRepository(IDbContextFactory<SizzlingHotProductsDbContext> factory) : IOrderRepository
{
    public async Task<IReadOnlyList<Order>> GetOrdersBetweenAsync(
        DateOnly fromInclusive,
        DateOnly toInclusive,
        CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var rows = await db.Orders
            .Include(o => o.Entries)
            .AsNoTracking()
            .Where(o => o.Date >= fromInclusive && o.Date <= toInclusive)
            .Where(o => o.Status == "completed")
            .ToListAsync(cancellationToken);

        return rows.Select(MapToDomainEntity).ToList();
    }

    public async Task<IReadOnlyList<Order>> GetCancellationsTargetingAsync(
        DateOnly fromInclusive,
        DateOnly toInclusive,
        CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var rows = await db.Orders
            .AsNoTracking()
            .Where(o => o.Status == "cancelled")
            .Where(o => o.OriginalOrderDate >= fromInclusive
                     && o.OriginalOrderDate <= toInclusive)
            .ToListAsync(cancellationToken);

        // Cancellation rows have no entries — acceptable; domain only needs the OrderId + Status
        return rows.Select(MapToDomainEntity).ToList();
    }

    private static Order MapToDomainEntity(OrderRow row) =>
        new(
            OrderId: row.OrderId,
            CustomerId: row.CustomerId,
            Date: row.Date,
            Status: row.Status == "cancelled" ? OrderStatus.Cancelled : OrderStatus.Completed,
            Entries: row.Entries
                .Select(e => new OrderEntry(e.ProductId, e.Quantity))
                .ToList());
}
