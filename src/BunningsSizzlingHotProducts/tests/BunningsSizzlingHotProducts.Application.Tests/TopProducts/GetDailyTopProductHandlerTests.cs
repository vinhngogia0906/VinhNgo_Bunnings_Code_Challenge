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
}
