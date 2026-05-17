using BunningsSizzlingHotProducts.Application.Abstractions;
using BunningsSizzlingHotProducts.Application.Queries;
using BunningsSizzlingHotProducts.Application.Results;
using BunningsSizzlingHotProducts.Domain.SalesAggregation;

namespace BunningsSizzlingHotProducts.Application.Handlers;

public sealed class GetRollingTopProductHandler(
    IOrderRepository orderRepo,
    IProductRepository productRepo,
    IClock clock)
{
    private readonly OrderReducer _reducer = new();
    private readonly ProductSaleCounter _counter = new();
    private readonly TopProductSelector _selector = new();

    public async Task<TopProductResult> HandleAsync(
        GetRollingTopProductQuery query,
        CancellationToken cancellationToken)
    {
        var to = clock.Today;
        var from = to.AddDays(-(query.Days - 1));

        // This is the other syntax of await Task.WhenAll
        var completed = await orderRepo.GetOrdersBetweenAsync(from, to, cancellationToken);
        var cancellations = await orderRepo.GetCancellationsTargetingAsync(from, to, cancellationToken);
        var products = await productRepo.GetAllAsync(cancellationToken);

        var reduced = _reducer.Reduce(completed.Concat(cancellations));
        var counts = _counter.Count(reduced)
            .Where(c => c.Date >= from && c.Date <= to);
        var topName = _selector.Select(counts, products);

        return new TopProductResult(from, to, topName);
    }
}
