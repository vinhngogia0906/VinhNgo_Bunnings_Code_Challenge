using BunningsSizzlingHotProducts.Domain.Entities;
using BunningsSizzlingHotProducts.Domain.SalesAggregation;
using FluentAssertions;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BunningsSizzlingHotProducts.Domain.Tests.SalesAggregation;

public class AggregationAcceptanceTests
{
    private static readonly Product[] Products =
    [
        new("P1", "Ezy Storage 37L Flexi Laundry Basket - White"),
        new("P2", "Aandleford Black Seaford Post Mounted Letterbox"),
        new("P3", "Coolaroo 5.4m Square Graphite Premium Shade Sail Kit"),
        new("P4", "Ozito 80W Soldering Iron"),
        new("P5", "Richgro 25L All Purpose Garden Soil Mix"),
        new("P6", "Arlec 160W Crystalline Solar Foldable Charging Kit")
    ];

    private static readonly Order[] Orders = BuildOrdersMatchingBrief();

    [Theory]
    [InlineData("21/04/2026", "Ezy Storage 37L Flexi Laundry Basket - White")]
    [InlineData("22/04/2026", "Ezy Storage 37L Flexi Laundry Basket - White")]
    [InlineData("23/04/2026", "Arlec 160W Crystalline Solar Foldable Charging Kit")]
    public void Top_product_per_day_matches_brief(string dateText, string expectedName)
    {
        var date = DateOnly.ParseExact(dateText, "dd/MM/yyyy");
        var reduced = new OrderReducer().Reduce(Orders);
        var counts = new ProductSaleCounter().Count(reduced)
            .Where(c => c.Date == date)
            .ToList();
        var top = new TopProductSelector().Select(counts, Products);

        top.Should().Be(expectedName);
    }

    [Fact]
    public void Top_product_for_three_day_window_matches_brief()
    {
        var from = new DateOnly(2026, 4, 21);
        var to = new DateOnly(2026, 4, 23);
        var reduced = new OrderReducer().Reduce(Orders);
        var counts = new ProductSaleCounter().Count(reduced)
            .Where(c => c.Date >= from && c.Date <= to)
            .ToList();
        var top = new TopProductSelector().Select(counts, Products);

        top.Should().Be("Ezy Storage 37L Flexi Laundry Basket - White");
    }

    private static Order[] BuildOrdersMatchingBrief()
    {

        var orders = new List<Order>() {
            new("O10", "C1", new DateOnly(2026,4,21), Enums.OrderStatus.Completed,
                [
                    new OrderEntry("P1", 1)
                ]
            ),
            new("O20", "C2", new DateOnly(2026,4,21), Enums.OrderStatus.Completed,
                [
                    new OrderEntry("P1", 1)
                ]
            ),
            new("O30", "C2", new DateOnly(2026,4,21), Enums.OrderStatus.Completed,
                [
                    new OrderEntry("P2", 1)
                ]
            ),
            new("O31", "C3", new DateOnly(2026,4,21), Enums.OrderStatus.Completed,
                [
                    new OrderEntry("P2", 1),
                    new OrderEntry("P1", 2)
                ]
            ),
            new("O32", "C32", new DateOnly(2026,4,21), Enums.OrderStatus.Completed,
                [
                    new OrderEntry("P2", 1),
                ]
            ),
            new("O30", "C2", new DateOnly(2026,4,22), Enums.OrderStatus.Cancelled,
                [
                    new OrderEntry("P2", 1),
                ]
            ),
            new("O40", "C3", new DateOnly(2026,4,22), Enums.OrderStatus.Completed,
                [
                    new OrderEntry("P4", 2),
                ]
            ),
            new("O60", "C3", new DateOnly(2026,4,22), Enums.OrderStatus.Completed,
                [
                    new OrderEntry("P4", 2),
                    new OrderEntry("P1", 2),
                ]
            ),
            new("O70", "C4", new DateOnly(2026,4,22), Enums.OrderStatus.Completed,
                [
                    new OrderEntry("P5", 2)
                ]
            ),
            new("O80", "C5", new DateOnly(2026,4,22), Enums.OrderStatus.Completed,
                [
                    new OrderEntry("P1", 2)
                ]
            ),
            new("O81", "C5", new DateOnly(2026,4,22), Enums.OrderStatus.Completed,
                [
                    new OrderEntry("P1", 10)
                ]
            ),
            new("O90", "C5", new DateOnly(2026,4,23), Enums.OrderStatus.Completed,
                [
                    new OrderEntry("P1", 1)
                ]
            ),
            new("O100", "C3", new DateOnly(2026,4,23), Enums.OrderStatus.Completed,
                [
                    new OrderEntry("P6", 3)
                ]
            ),
        };
        return orders.ToArray();
    }
}
