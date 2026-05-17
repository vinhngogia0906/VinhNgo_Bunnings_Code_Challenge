using BunningsSizzlingHotProducts.Domain.Entities;
using BunningsSizzlingHotProducts.Domain.Enums;
using BunningsSizzlingHotProducts.Domain.SalesAggregation;
using FluentAssertions;

namespace BunningsSizzlingHotProducts.Domain.Tests.SalesAggregation;

public class ProductSaleCounterTests
{
    [Fact]
    public void Quantity_greater_than_one_in_single_order_counts_as_one_sale()
    {
        // Arrange
        var orders = new[]
        {
            new Order(
                OrderId: "O10",
                CustomerId: "C1",
                Date: new DateOnly(2026, 4, 21),
                Status: OrderStatus.Completed,
                Entries: [new OrderEntry("P1", Quantity: 5)])
        };

        // Act
        var counter = new ProductSaleCounter();
        var counts = counter.Count(orders);

        // Assert
        counts.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { ProductId = "P1", Date = new DateOnly(2026, 4, 21), Sales = 1 });
    }

    [Fact]
    public void Same_customer_same_product_same_day_in_two_orders_counts_as_one_sale()
    {
        var orders = new[]
        {
        new Order("O10", "C1", new DateOnly(2026, 4, 21), OrderStatus.Completed,
            [new OrderEntry("P1", 2)]),
        new Order("O11", "C1", new DateOnly(2026, 4, 21), OrderStatus.Completed,
            [new OrderEntry("P1", 3)])
    };

        var counts = new ProductSaleCounter().Count(orders);

        counts.Should().ContainSingle()
            .Which.Sales.Should().Be(1);
    }

    [Fact]
    public void Empty_input_returns_empty()
    {
        var counts = new ProductSaleCounter().Count(Array.Empty<Order>());

        counts.Should().BeEmpty();
    }

    [Fact]
    public void Cancelled_orders_are_not_counted()
    {
        var orders = new[]
        {
            new Order("O10", "C1", new DateOnly(2026, 4, 21), OrderStatus.Cancelled,
                [new OrderEntry("P1", 1)])
        };

        var counts = new ProductSaleCounter().Count(orders);

        counts.Should().BeEmpty("cancellation rows must never contribute to counts");
    }

    [Fact]
    public void Different_customers_same_product_same_day_count_as_separate_sales()
    {
        var orders = new[]
        {
            new Order("O10", "C1", new DateOnly(2026, 4, 21), OrderStatus.Completed,
                [new OrderEntry("P1", 1)]),
            new Order("O11", "C2", new DateOnly(2026, 4, 21), OrderStatus.Completed,
                [new OrderEntry("P1", 1)])
        };

        var counts = new ProductSaleCounter().Count(orders);

        counts.Should().ContainSingle()
            .Which.Sales.Should().Be(2);
    }

    [Fact]
    public void Same_customer_same_product_different_days_count_as_separate_sales()
    {
        var orders = new[]
        {
            new Order("O10", "C1", new DateOnly(2026, 4, 21), OrderStatus.Completed,
                [new OrderEntry("P1", 1)]),
            new Order("O11", "C1", new DateOnly(2026, 4, 22), OrderStatus.Completed,
                [new OrderEntry("P1", 1)])
        };

        var counts = new ProductSaleCounter().Count(orders);

        counts.Should().HaveCount(2);
        counts.Should().AllSatisfy(c => c.Sales.Should().Be(1));
    }
}
