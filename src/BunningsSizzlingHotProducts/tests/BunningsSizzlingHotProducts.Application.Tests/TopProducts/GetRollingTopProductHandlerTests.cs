using BunningsSizzlingHotProducts.Application.Abstractions;
using BunningsSizzlingHotProducts.Application.Handlers;
using BunningsSizzlingHotProducts.Application.Queries;
using BunningsSizzlingHotProducts.Application.Tests.Support;
using BunningsSizzlingHotProducts.Domain.Entities;
using BunningsSizzlingHotProducts.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BunningsSizzlingHotProducts.Application.Tests.TopProducts;

public class GetRollingTopProductHandlerTests
{
    private static readonly DateOnly Today = new(2026, 4, 23);

    [Fact]
    public async Task Returns_top_product_across_a_three_day_rolling_window()
    {
        var (orderRepo, productRepo) = BuildRepos(
            completed:
            [
                new Order("O1", "C1", new DateOnly(2026, 4, 21), OrderStatus.Completed,
                    [new OrderEntry("P1", 1)]),
                new Order("O2", "C2", new DateOnly(2026, 4, 22), OrderStatus.Completed,
                    [new OrderEntry("P1", 1)]),
                new Order("O3", "C3", new DateOnly(2026, 4, 23), OrderStatus.Completed,
                    [new OrderEntry("P2", 1)]),
            ],
            cancellations: [],
            products:
            [
                new Product("P1", "Hammer"),
                new Product("P2", "BBQ"),
            ]);

        var handler = new GetRollingTopProductHandler(
            orderRepo.Object, productRepo.Object, new FixedClock(Today));

        var result = await handler.HandleAsync(new GetRollingTopProductQuery(3), CancellationToken.None);

        result.From.Should().Be(new DateOnly(2026, 4, 21));
        result.To.Should().Be(Today);
        result.ProductName.Should().Be("Hammer");
    }

    [Fact]
    public async Task Single_day_window_returns_only_today_sales()
    {
        var (orderRepo, productRepo) = BuildRepos(
            completed:
            [
                new Order("O1", "C1", new DateOnly(2026, 4, 22), OrderStatus.Completed,
                    [new OrderEntry("P1", 1)]),
                new Order("O2", "C2", Today, OrderStatus.Completed,
                    [new OrderEntry("P2", 1)]),
            ],
            cancellations: [],
            products:
            [
                new Product("P1", "Hammer"),
                new Product("P2", "BBQ"),
            ]);

        var handler = new GetRollingTopProductHandler(
            orderRepo.Object, productRepo.Object, new FixedClock(Today));

        var result = await handler.HandleAsync(new GetRollingTopProductQuery(1), CancellationToken.None);

        result.From.Should().Be(Today);
        result.To.Should().Be(Today);
        result.ProductName.Should().Be("BBQ");
    }

    [Fact]
    public async Task Empty_window_returns_null_product_name()
    {
        var (orderRepo, productRepo) = BuildRepos(completed: [], cancellations: [], products: []);

        var handler = new GetRollingTopProductHandler(
            orderRepo.Object, productRepo.Object, new FixedClock(Today));

        var result = await handler.HandleAsync(new GetRollingTopProductQuery(7), CancellationToken.None);

        result.ProductName.Should().BeNull();
        result.From.Should().Be(new DateOnly(2026, 4, 17));
        result.To.Should().Be(Today);
    }

    [Fact]
    public async Task Cancellation_targeting_a_day_inside_the_window_removes_that_sale()
    {
        // Without the cancellation P1 wins (2 customers). With it, only one P1 sale remains
        // and P2's two customers win the window.
        var (orderRepo, productRepo) = BuildRepos(
            completed:
            [
                new Order("O1", "C1", new DateOnly(2026, 4, 21), OrderStatus.Completed,
                    [new OrderEntry("P1", 1)]),
                new Order("O2", "C2", new DateOnly(2026, 4, 22), OrderStatus.Completed,
                    [new OrderEntry("P1", 1)]),
                new Order("O3", "C3", new DateOnly(2026, 4, 22), OrderStatus.Completed,
                    [new OrderEntry("P2", 1)]),
                new Order("O4", "C4", new DateOnly(2026, 4, 23), OrderStatus.Completed,
                    [new OrderEntry("P2", 1)]),
            ],
            cancellations:
            [
                new Order("O1", "C1", new DateOnly(2026, 4, 22), OrderStatus.Cancelled,
                    Entries: []),
            ],
            products:
            [
                new Product("P1", "Hammer"),
                new Product("P2", "BBQ"),
            ]);

        var handler = new GetRollingTopProductHandler(
            orderRepo.Object, productRepo.Object, new FixedClock(Today));

        var result = await handler.HandleAsync(new GetRollingTopProductQuery(3), CancellationToken.None);

        result.ProductName.Should().Be("BBQ");
    }

    private static (Mock<IOrderRepository> Order, Mock<IProductRepository> Product) BuildRepos(
        IReadOnlyList<Order> completed,
        IReadOnlyList<Order> cancellations,
        IReadOnlyList<Product> products)
    {
        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(r => r.GetOrdersBetweenAsync(
                It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(completed);
        orderRepo.Setup(r => r.GetCancellationsTargetingAsync(
                It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cancellations);

        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        return (orderRepo, productRepo);
    }
}
