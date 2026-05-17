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

    [Fact]
    public void Empty_input_returns_empty()
    {
        var effective = new OrderReducer().Reduce(Array.Empty<Order>());

        effective.Should().BeEmpty();
    }

    [Fact]
    public void Cancellation_without_matching_completed_is_silently_dropped()
    {
        var raw = new[]
        {
            new Order("O99", "C9", new DateOnly(2026, 4, 22), OrderStatus.Cancelled,
                Entries: [])
        };

        var effective = new OrderReducer().Reduce(raw);

        effective.Should().BeEmpty(
            "an orphan cancellation has nothing to cancel and is not itself a sale");
    }

    [Fact]
    public void Duplicate_completed_for_same_OrderId_keeps_last_write()
    {
        var earlier = new Order("O10", "C1", new DateOnly(2026, 4, 21), OrderStatus.Completed,
            [new OrderEntry("P1", 1)]);
        var later = new Order("O10", "C1", new DateOnly(2026, 4, 21), OrderStatus.Completed,
            [new OrderEntry("P2", 1)]);

        var effective = new OrderReducer().Reduce([earlier, later]);

        effective.Should().ContainSingle()
            .Which.Entries.Should().ContainSingle()
            .Which.ProductId.Should().Be("P2", "last completed write wins per the reducer contract");
    }
}
