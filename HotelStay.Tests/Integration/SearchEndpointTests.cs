using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelStay.Tests.Integration;

[Collection("Integration")]
public class SearchEndpointTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _client = null!;

    public SearchEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        await AuthorizeAsync(_client);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task AuthorizeAsync(HttpClient client)
    {
        var res = await client.PostAsJsonAsync("/auth/register",
            new { email = $"search_{Guid.NewGuid():N}@test.com", password = "Password1" });
        var token = await res.Content.ReadFromJsonAsync<TestTokenResponse>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);
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

    [Fact]
    public async Task GetSearch_WithRoomTypeFilter_ReturnsOnlyMatchingRooms()
    {
        var response = await _client.GetAsync(
            "/hotels/search?destination=Oslo&checkIn=2025-10-01&checkOut=2025-10-05&roomType=Standard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);

        var results = doc.RootElement.GetProperty("results");
        results.GetArrayLength().Should().BeGreaterThan(0);

        foreach (var room in results.EnumerateArray())
        {
            room.GetProperty("roomType").GetString().Should().Be("Standard");
        }
    }

    private record TestTokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);
}
