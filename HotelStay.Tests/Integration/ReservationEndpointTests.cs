using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using HotelStay.Application.DTOs;
using HotelStay.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelStay.Tests.Integration;

public class ReservationEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public ReservationEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private static ReserveRequestDto BuildRequest(
        string destination = "Paris",
        DocumentType docType = DocumentType.Passport) =>
        new(
            Provider: "PremierStays",
            RoomType: RoomType.Standard,
            Destination: destination,
            CheckIn: new DateOnly(2025, 11, 1),
            CheckOut: new DateOnly(2025, 11, 5),
            PerNightRate: 99m,
            CancellationPolicy: CancellationPolicy.FreeCancellation,
            GuestName: "Jane Smith",
            DocumentType: docType,
            DocumentNumber: "P12345678");

    [Fact]
    public async Task PostReserve_PassportForParis_Returns201WithReference()
    {
        var response = await _client.PostAsJsonAsync("/hotels/reserve", BuildRequest(), _jsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        var detail = JsonSerializer.Deserialize<ReservationDetailDto>(body, _jsonOptions);

        detail.Should().NotBeNull();
        detail!.Reference.Should().StartWith("HS-");
    }

    [Fact]
    public async Task PostReserve_NationalIdForParis_Returns422()
    {
        var response = await _client.PostAsJsonAsync("/hotels/reserve",
            BuildRequest(destination: "Paris", docType: DocumentType.NationalId), _jsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetReservation_ValidReference_Returns200()
    {
        // Create reservation first
        var postResponse = await _client.PostAsJsonAsync("/hotels/reserve", BuildRequest(), _jsonOptions);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await postResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<ReservationDetailDto>(body, _jsonOptions)!;

        // Retrieve it
        var getResponse = await _client.GetAsync($"/hotels/reservation/{created.Reference}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getBody = await getResponse.Content.ReadAsStringAsync();
        var fetched = JsonSerializer.Deserialize<ReservationDetailDto>(getBody, _jsonOptions);

        fetched!.Reference.Should().Be(created.Reference);
    }

    [Fact]
    public async Task GetReservation_FakeReference_Returns404()
    {
        var response = await _client.GetAsync("/hotels/reservation/HS-FAKEX");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
