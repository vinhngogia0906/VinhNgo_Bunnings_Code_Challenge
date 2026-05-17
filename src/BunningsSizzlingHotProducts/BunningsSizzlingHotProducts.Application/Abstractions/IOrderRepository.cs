using BunningsSizzlingHotProducts.Domain.Entities;

namespace BunningsSizzlingHotProducts.Application.Abstractions;

public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> GetOrdersBetweenAsync(
        DateOnly fromInclusive,
        DateOnly toInclusive,
        CancellationToken cancellationToken);

    // Cancellations may arrive AFTER the day we are querying, so we need this
    // to be able to get cancellations that target orders in the date range we are querying
    Task<IReadOnlyList<Order>> GetCancellationsTargetingAsync(
        DateOnly fromInclusive,
        DateOnly toInclusive,
        CancellationToken cancellationToken);
}
