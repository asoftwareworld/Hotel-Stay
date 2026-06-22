using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelStay.Tests.Integration;

[Collection("Integration")]
public class AuthEndpointTests
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Creates a new test-server-connected client, registers a fresh user, and sets its Bearer header
    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var res = await client.PostAsJsonAsync("/auth/register",
            new { email = $"auth_{Guid.NewGuid():N}@test.com", password = "Password1" });
        var token = await res.Content.ReadFromJsonAsync<TokenResponse>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }

    // ── Register ─────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidBody_Returns201WithTokens()
    {
        var res = await _client.PostAsJsonAsync("/auth/register",
            new { email = $"new_{Guid.NewGuid():N}@test.com", password = "Password1" });

        res.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await res.Content.ReadFromJsonAsync<TokenResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var email = $"dup_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/auth/register",
            new { email, password = "Password1" });

        var res = await _client.PostAsJsonAsync("/auth/register",
            new { email, password = "Password1" });

        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400()
    {
        var res = await _client.PostAsJsonAsync("/auth/register",
            new { email = "valid@test.com", password = "short" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_MissingEmail_Returns400()
    {
        var res = await _client.PostAsJsonAsync("/auth/register",
            new { email = "", password = "Password1" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Login ─────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokens()
    {
        var email = $"login_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/auth/register",
            new { email, password = "Password1" });

        var res = await _client.PostAsJsonAsync("/auth/login",
            new { email, password = "Password1" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<TokenResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var email = $"bad_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/auth/register",
            new { email, password = "Password1" });

        var res = await _client.PostAsJsonAsync("/auth/login",
            new { email, password = "WrongPass1" });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401()
    {
        var res = await _client.PostAsJsonAsync("/auth/login",
            new { email = "nobody@nowhere.com", password = "Password1" });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Refresh ───────────────────────────────────────────────

    [Fact]
    public async Task Refresh_ValidToken_Returns200WithNewTokens()
    {
        var email = $"refresh_{Guid.NewGuid():N}@test.com";
        var regRes = await _client.PostAsJsonAsync("/auth/register",
            new { email, password = "Password1" });
        var reg = await regRes.Content.ReadFromJsonAsync<TokenResponse>();

        var res = await _client.PostAsJsonAsync("/auth/refresh",
            new { refreshToken = reg!.RefreshToken });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<TokenResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBe(reg.RefreshToken); // rotated
    }

    [Fact]
    public async Task Refresh_InvalidToken_Returns401()
    {
        var res = await _client.PostAsJsonAsync("/auth/refresh",
            new { refreshToken = "totally-fake-token" });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ConsumedToken_Returns401()
    {
        var email = $"consume_{Guid.NewGuid():N}@test.com";
        var regRes = await _client.PostAsJsonAsync("/auth/register",
            new { email, password = "Password1" });
        var reg = await regRes.Content.ReadFromJsonAsync<TokenResponse>();

        await _client.PostAsJsonAsync("/auth/refresh",
            new { refreshToken = reg!.RefreshToken });

        var res = await _client.PostAsJsonAsync("/auth/refresh",
            new { refreshToken = reg.RefreshToken });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Logout ────────────────────────────────────────────────

    [Fact]
    public async Task Logout_AuthenticatedUser_Returns204()
    {
        var authedClient = await CreateAuthenticatedClientAsync();
        var email = $"logout_{Guid.NewGuid():N}@test.com";
        var regRes = await _client.PostAsJsonAsync("/auth/register",
            new { email, password = "Password1" });
        var reg = await regRes.Content.ReadFromJsonAsync<TokenResponse>();

        authedClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", reg!.AccessToken);

        var res = await authedClient.PostAsJsonAsync("/auth/logout",
            new { refreshToken = reg.RefreshToken });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_Unauthenticated_Returns401()
    {
        var res = await _client.PostAsJsonAsync("/auth/logout",
            new { refreshToken = "any" });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Protected hotel endpoints require auth ────────────────

    [Fact]
    public async Task HotelSearch_WithoutToken_Returns401()
    {
        var res = await _client.GetAsync("/hotels/search?destination=Paris&checkIn=2025-08-01&checkOut=2025-08-05");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HotelSearch_WithValidToken_Returns200()
    {
        var authedClient = await CreateAuthenticatedClientAsync();

        var res = await authedClient.GetAsync(
            "/hotels/search?destination=Paris&checkIn=2025-08-01&checkOut=2025-08-05");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private record TokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);
}
