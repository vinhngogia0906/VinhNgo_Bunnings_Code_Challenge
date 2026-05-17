using BunningsSizzlingHotProducts.Domain.Enums;

namespace BunningsSizzlingHotProducts.Domain.Entities;

public sealed record Order(
    string OrderId,
    string CustomerId,
    DateOnly Date,
    OrderStatus Status,
    IReadOnlyList<OrderEntry> Entries);
