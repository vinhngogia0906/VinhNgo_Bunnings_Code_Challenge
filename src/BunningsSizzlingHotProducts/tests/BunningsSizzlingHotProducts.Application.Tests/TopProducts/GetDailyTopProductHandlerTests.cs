using BunningsSizzlingHotProducts.Application.Abstractions;
using BunningsSizzlingHotProducts.Application.Handlers;
using BunningsSizzlingHotProducts.Application.Queries;
using BunningsSizzlingHotProducts.Application.Results;
using BunningsSizzlingHotProducts.Domain.Entities;
using BunningsSizzlingHotProducts.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BunningsSizzlingHotProducts.Application.Tests.TopProducts;

public class GetDailyTopProductHandlerTests
{
    [Fact]
    public async Task Returns_top_product_for_a_specific_day()
    {
        // Arrange
        var date = new DateOnly(2026, 4, 21);

        var orders = new[]
        {
            new Order("O1", "C1", date, OrderStatus.Completed, [new OrderEntry("P1", 1)]),
            new Order("O2", "C2", date, OrderStatus.Completed, [new OrderEntry("P1", 1)]),
            new Order("O3", "C3", date, OrderStatus.Completed, [new OrderEntry("P2", 1)])
        };

        var products = new[]
        {
            new Product("P1", "Hammer"),
            new Product("P2", "BBQ")
        };

        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(r => r.GetOrdersBetweenAsync(date, date, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(orders);
        orderRepo.Setup(r => r.GetCancellationsTargetingAsync(date, date, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(Array.Empty<Order>());

        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(products);

        var handler = new GetDailyTopProductHandler(orderRepo.Object, productRepo.Object);

        // Act
        var result = await handler.HandleAsync(new GetDailyTopProductQuery(date), CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new TopProductResult(date, date, "Hammer"));
    }

    [Fact]
    public async Task Returns_null_product_name_when_no_orders_for_the_day()
    {
        var date = new DateOnly(2026, 4, 21);

        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(r => r.GetOrdersBetweenAsync(date, date, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(Array.Empty<Order>());
        orderRepo.Setup(r => r.GetCancellationsTargetingAsync(date, date, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(Array.Empty<Order>());

        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Array.Empty<Product>());

        var handler = new GetDailyTopProductHandler(orderRepo.Object, productRepo.Object);

        var result = await handler.HandleAsync(new GetDailyTopProductQuery(date), CancellationToken.None);

        result.Should().BeEquivalentTo(new TopProductResult(date, date, null));
    }

    [Fact]
    public async Task Cancellation_targeting_the_day_removes_its_sale_from_the_total()
    {
        var date = new DateOnly(2026, 4, 21);

        var completed = new[]
        {
            new Order("O1", "C1", date, OrderStatus.Completed, [new OrderEntry("P1", 1)]),
            new Order("O2", "C2", date, OrderStatus.Completed, [new OrderEntry("P2", 1)]),
        };
        var cancellations = new[]
        {
            new Order("O1", "C1", date.AddDays(1), OrderStatus.Cancelled, Entries: []),
        };
        var products = new[]
        {
            new Product("P1", "Hammer"),
            new Product("P2", "BBQ"),
        };

        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(r => r.GetOrdersBetweenAsync(date, date, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(completed);
        orderRepo.Setup(r => r.GetCancellationsTargetingAsync(date, date, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(cancellations);

        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(products);

        var handler = new GetDailyTopProductHandler(orderRepo.Object, productRepo.Object);

        var result = await handler.HandleAsync(new GetDailyTopProductQuery(date), CancellationToken.None);

        result.ProductName.Should().Be("BBQ", "the cancellation removes P1's only sale for the day");
    }
}
