namespace BunningsSizzlingHotProducts.Infrastructure.Persistence.Models;

public sealed class OrderRow
{
    public required string OrderId { get; init; }
    public required string CustomerId { get; init; }
    public required DateOnly Date { get; set; } // mutable: cancellation rewrites this
    public required string Status { get; set; } // "completed" | "cancelled"
    public DateOnly? OriginalOrderDate { get; set; }    // populated for cancellation rows
    public List<OrderEntryRow> Entries { get; init; } = [];
}
