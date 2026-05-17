namespace BunningsSizzlingHotProducts.Infrastructure.Seeding;

internal sealed record SeedProduct(string Id, string Name);
internal sealed record SeedOrderEntry(string Id, int Quantity);
internal sealed record SeedOrder(
    string OrderId,
    string CustomerId,
    SeedOrderEntry[]? Entries,
    string Date,
    string Status);
