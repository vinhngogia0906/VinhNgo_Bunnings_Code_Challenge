namespace BunningsSizzlingHotProducts.Domain.Time;

public interface IClock
{
    DateOnly Today { get; }
}
