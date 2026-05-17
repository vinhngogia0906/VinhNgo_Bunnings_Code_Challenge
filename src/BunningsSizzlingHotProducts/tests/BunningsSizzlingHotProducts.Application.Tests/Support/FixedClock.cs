using BunningsSizzlingHotProducts.Application.Abstractions;

namespace BunningsSizzlingHotProducts.Application.Tests.Support;

public sealed class FixedClock(DateOnly today) : IClock
{
    public DateOnly Today { get; } = today;
}
