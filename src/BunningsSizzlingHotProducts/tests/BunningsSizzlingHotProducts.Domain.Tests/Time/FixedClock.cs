using BunningsSizzlingHotProducts.Domain.Time;

namespace BunningsSizzlingHotProducts.Domain.Tests.Time;

internal sealed class FixedClock(DateOnly today) : IClock
{
    public DateOnly Today { get; } = today;
}
