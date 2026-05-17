using BunningsSizzlingHotProducts.Application.Abstractions;

namespace BunningsSizzlingHotProducts.Infrastructure;

public sealed class SystemClock : IClock
{
    // The brief fixes "today" to 23/04/2026. In real application, this would be DateOnly.FromDateTime(DateTime.UtcNow).
    public DateOnly Today => new(2026, 4, 23);
}
