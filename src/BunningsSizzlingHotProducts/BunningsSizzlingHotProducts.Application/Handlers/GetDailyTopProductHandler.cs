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
        var completedTask = orderRepo.GetOrdersBetweenAsync(query.Date, query.Date, cancellationToken);
        var cancellationsTask = orderRepo.GetCancellationsTargetingAsync(query.Date, query.Date, cancellationToken);
        var productsTask = productRepo.GetAllAsync(cancellationToken);

        await Task.WhenAll(completedTask, cancellationsTask, productsTask);

        var allOrders = completedTask.Result.Concat(cancellationsTask.Result);
        var reduced = _reducer.Reduce(allOrders);
        var counts = _counter.Count(reduced);
        var topName = _selector.Select(counts, productsTask.Result);

        return new TopProductResult(query.Date, query.Date, topName);
    }
}
