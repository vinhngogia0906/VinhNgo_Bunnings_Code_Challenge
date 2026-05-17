namespace BunningsSizzlingHotProducts.Application.Results;

public sealed record TopProductResult(DateOnly From, DateOnly To, string? ProductName);
