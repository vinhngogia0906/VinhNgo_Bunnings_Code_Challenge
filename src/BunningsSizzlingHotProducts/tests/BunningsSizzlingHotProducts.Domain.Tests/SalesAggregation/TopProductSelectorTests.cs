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

    [Fact]
    public void Empty_sales_returns_null()
    {
        var products = new[] { new Product("P1", "Hammer") };

        var top = new TopProductSelector().Select(Array.Empty<ProductDailySale>(), products);

        top.Should().BeNull();
    }

    [Fact]
    public void Single_product_with_single_sale_wins()
    {
        var products = new[] { new Product("P1", "Hammer") };
        var sales = new[] { new ProductDailySale("P1", new DateOnly(2026, 4, 21), 1) };

        var top = new TopProductSelector().Select(sales, products);

        top.Should().Be("Hammer");
    }

    [Fact]
    public void Product_missing_from_catalog_falls_back_to_id()
    {
        var sales = new[] { new ProductDailySale("PXX", new DateOnly(2026, 4, 21), 1) };

        var top = new TopProductSelector().Select(sales, Array.Empty<Product>());

        top.Should().Be("PXX", "the selector falls back to ProductId when no name is available");
    }

    [Fact]
    public void Sales_for_same_product_across_dates_are_summed()
    {
        var products = new[]
        {
            new Product("P1", "Hammer"),
            new Product("P2", "BBQ")
        };
        var sales = new[]
        {
            new ProductDailySale("P1", new DateOnly(2026, 4, 21), 2),
            new ProductDailySale("P1", new DateOnly(2026, 4, 22), 2),
            new ProductDailySale("P2", new DateOnly(2026, 4, 22), 3),
        };

        var top = new TopProductSelector().Select(sales, products);

        top.Should().Be("Hammer", "P1 totals 4 across two days vs P2's 3 on one day");
    }
}
