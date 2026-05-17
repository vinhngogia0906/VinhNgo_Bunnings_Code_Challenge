using BunningsSizzlingHotProducts.Domain.Entities;
using BunningsSizzlingHotProducts.Domain.SalesAggregation;
using FluentAssertions;

namespace BunningsSizzlingHotProducts.Domain.Tests.SalesAggregation;

public class TopProductSelectorTests
{
    [Fact]
    public void On_a_tie_alphabetically_first_product_name_wins()
    {
        var products = new[]
        {
            new Product("P1", "Hammer"),
            new Product("P2", "BBQ")
        };
        var dailySales = new[]
        {
            new ProductDailySale("P1", new DateOnly(2026, 4, 21), 5),
            new ProductDailySale("P2", new DateOnly(2026, 4, 21), 5)
        };

        var top = new TopProductSelector().Select(dailySales, products);

        top.Should().Be("BBQ");
    }

    [Fact]
    public void Highest_sales_wins_regardless_of_alphabet()
    {
        var products = new[]
        {
            new Product("P1", "Hammer"),
            new Product("P2", "BBQ")
        };
        var dailySales = new[]
        {
            new ProductDailySale("P1", new DateOnly(2026, 4, 21), 7),
            new ProductDailySale("P2", new DateOnly(2026, 4, 21), 5)
        };

        var top = new TopProductSelector().Select(dailySales, products);

        top.Should().Be("Hammer");
    }
}
