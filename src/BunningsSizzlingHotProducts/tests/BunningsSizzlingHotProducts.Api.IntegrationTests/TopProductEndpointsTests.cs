using BunningsSizzlingHotProducts.Api.Contracts;
using BunningsSizzlingHotProducts.Api.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace BunningsSizzlingHotProducts.Api.IntegrationTests;

public sealed class TopProductEndpointsTests : IClassFixture<PostgresWebAppFactory>
{
    private readonly HttpClient _client;

    public TopProductEndpointsTests(PostgresWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Daily_returns_top_product_when_seeded()
    {
        var resp = await _client.GetAsync("/api/top-product/daily?date=2026-04-21");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await resp.Content.ReadFromJsonAsync<TopProductResponse>();
        payload!.ProductName.Should().Be("Ezy Storage 37L Flexi Laundry Basket - White");
    }

    [Fact]
    public async Task Daily_rejects_future_date()
    {
        var resp = await _client.GetAsync("/api/top-product/daily?date=2099-01-01");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Daily_rejects_default_date()
    {
        var resp = await _client.GetAsync("/api/top-product/daily?date=0001-01-01");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Rolling_returns_a_payload_for_the_default_three_day_window()
    {
        var resp = await _client.GetAsync("/api/top-product/rolling");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await resp.Content.ReadFromJsonAsync<TopProductResponse>();
        payload.Should().NotBeNull();
        payload!.From.Should().BeOnOrBefore(payload.To);
    }

    [Theory]
    [InlineData("/api/top-product/rolling?days=0")]
    [InlineData("/api/top-product/rolling?days=-1")]
    [InlineData("/api/top-product/rolling?days=366")]
    public async Task Rolling_rejects_out_of_range_days(string url)
    {
        var resp = await _client.GetAsync(url);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
