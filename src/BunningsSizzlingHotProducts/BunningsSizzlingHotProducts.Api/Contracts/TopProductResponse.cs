namespace BunningsSizzlingHotProducts.Api.Contracts;

public sealed record TopProductResponse(DateOnly From, DateOnly To, string? ProductName);
