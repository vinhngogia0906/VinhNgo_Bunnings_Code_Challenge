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
}
