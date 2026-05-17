using BunningsSizzlingHotProducts.Domain.Entities;
using BunningsSizzlingHotProducts.Domain.Enums;
using BunningsSizzlingHotProducts.Domain.SalesAggregation;
using FluentAssertions;

namespace BunningsSizzlingHotProducts.Domain.Tests.SalesAggregation;

public class OrderReducerTests
{
    [Fact]
    public void Cancellation_removes_the_original_completed_order_from_results()
    {
        var raw = new[]
        {
            new Order("O10", "C1", new DateOnly(2026, 4, 21), OrderStatus.Completed,
                [new OrderEntry("P1", 1)]),
            new Order("O10", "C1", new DateOnly(2026, 4, 22), OrderStatus.Cancelled,
                Entries: []) // cancellation arrives a day later with the same OrderId
        };

        var effective = new OrderReducer().Reduce(raw);

        effective.Should().BeEmpty(
            "the cancellation must reverse the original O10 placed on 21/04/2026");
    }

    [Fact]
    public void Completed_orders_without_cancellations_pass_through_unchanged()
    {
        var raw = new[]
        {
            new Order("O20", "C2", new DateOnly(2026, 4, 21), OrderStatus.Completed,
                [new OrderEntry("P1", 1)])
        };

        var effective = new OrderReducer().Reduce(raw);

        effective.Should().HaveCount(1);
    }
}
