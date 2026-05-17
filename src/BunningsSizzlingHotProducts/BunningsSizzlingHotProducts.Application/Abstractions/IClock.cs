namespace BunningsSizzlingHotProducts.Application.Abstractions;

public interface IClock
{
    DateOnly Today { get; }
}
