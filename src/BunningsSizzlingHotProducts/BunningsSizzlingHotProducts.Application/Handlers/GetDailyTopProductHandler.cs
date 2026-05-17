using BunningsSizzlingHotProducts.Application.Abstractions;
using BunningsSizzlingHotProducts.Application.Queries;
using BunningsSizzlingHotProducts.Application.Results;
using BunningsSizzlingHotProducts.Domain.SalesAggregation;

namespace BunningsSizzlingHotProducts.Application.Handlers;

public sealed class GetDailyTopProductHandler(
    IOrderRepository orderRepo,
    IProductRepository productRepo)
{
    private readonly OrderReducer _reducer = new();
    private readonly ProductSaleCounter _counter = new();
    private readonly TopProductSelector _selector = new();

    public async Task<TopProductResult> HandleAsync(
        GetDailyTopProductQuery query,
        CancellationToken cancellationToken)
    {
        var completed = await orderRepo.GetOrdersBetweenAsync(query.Date, query.Date, cancellationToken);
        var cancellations = await orderRepo.GetCancellationsTargetingAsync(query.Date, query.Date, cancellationToken);
        var products = await productRepo.GetAllAsync(cancellationToken);

        var allOrders = completed.Concat(cancellations);
        var reduced = _reducer.Reduce(allOrders);
        var counts = _counter.Count(reduced);
        var topName = _selector.Select(counts, products);

        return new TopProductResult(query.Date, query.Date, topName);
    }
}
