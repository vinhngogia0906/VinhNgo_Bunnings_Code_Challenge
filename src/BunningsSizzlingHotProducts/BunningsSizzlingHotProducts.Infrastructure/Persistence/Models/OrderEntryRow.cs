namespace BunningsSizzlingHotProducts.Infrastructure.Persistence.Models;

public sealed class OrderEntryRow
{
    public int Id { get; set; } // surrogate PK
    public required string OrderId { get; init; } // FK of Order table
    public required string ProductId { get; init; }
    public required int Quantity { get; init; }
}
