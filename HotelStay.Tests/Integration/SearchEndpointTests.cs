using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelStay.Tests.Integration;

[Collection("Integration")]
public class SearchEndpointTests
{
    private readonly HttpClient _client;

    public SearchEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSearch_MissingDestination_Returns400()
    {
        var response = await _client.GetAsync("/hotels/search?checkIn=2025-10-01&checkOut=2025-10-05");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSearch_CheckOutBeforeCheckIn_Returns400()
    {
        var response = await _client.GetAsync("/hotels/search?destination=Oslo&checkIn=2025-10-05&checkOut=2025-10-01");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSearch_UnknownDestination_Returns400()
    {
        var response = await _client.GetAsync("/hotels/search?destination=FakeCity&checkIn=2025-10-01&checkOut=2025-10-05");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSearch_ValidParisSearch_Returns200WithResultsFromBothProviders()
    {
        var response = await _client.GetAsync("/hotels/search?destination=Paris&checkIn=2025-10-01&checkOut=2025-10-05");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);

        var results = doc.RootElement.GetProperty("results");
        results.GetArrayLength().Should().BeGreaterThan(0);

        var providers = results.EnumerateArray()
            .Select(r => r.GetProperty("provider").GetString())
            .Distinct()
            .ToList();

        providers.Should().Contain("PremierStays");
        providers.Should().Contain("BudgetNests");
    }
}
