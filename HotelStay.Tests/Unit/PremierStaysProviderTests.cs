using FluentAssertions;
using HotelStay.Domain.Enums;
using HotelStay.Domain.ValueObjects;
using HotelStay.Infrastructure.Providers;
using Xunit;

namespace HotelStay.Tests.Unit;

public class PremierStaysProviderTests
{
    private readonly PremierStaysProvider _sut = new();

    private static SearchCriteria BuildCriteria(RoomType? roomType = null, int nights = 3) =>
        new(
            Destination: "Oslo",
            CheckIn: new DateOnly(2025, 10, 1),
            CheckOut: new DateOnly(2025, 10, 1).AddDays(nights),
            RoomType: roomType);

    [Fact]
    public async Task SearchAsync_NoFilter_ReturnsAllThreeRooms()
    {
        var results = await _sut.SearchAsync(BuildCriteria());

        results.Should().HaveCount(3);
        results.Select(r => r.RoomType).Should().BeEquivalentTo(
            [RoomType.Standard, RoomType.Deluxe, RoomType.Suite]);
    }

    [Fact]
    public async Task SearchAsync_WithSuiteFilter_ReturnsOnlySuite()
    {
        var results = await _sut.SearchAsync(BuildCriteria(RoomType.Suite));

        results.Should().ContainSingle();
        results[0].RoomType.Should().Be(RoomType.Suite);
    }

    [Fact]
    public async Task SearchAsync_WithDeluxeFilter_ReturnsOnlyDeluxe()
    {
        var results = await _sut.SearchAsync(BuildCriteria(RoomType.Deluxe));

        results.Should().ContainSingle();
        results[0].RoomType.Should().Be(RoomType.Deluxe);
    }

    [Fact]
    public async Task SearchAsync_TotalPrice_IsRateTimesNights()
    {
        var nights = 4;
        var results = await _sut.SearchAsync(BuildCriteria(nights: nights));

        results.Should().AllSatisfy(r =>
            r.TotalPrice.Should().Be(r.PerNightRate * nights));
    }

    [Fact]
    public async Task SearchAsync_AllRooms_HaveAmenitiesAndStarRating()
    {
        var results = await _sut.SearchAsync(BuildCriteria());

        results.Should().AllSatisfy(r =>
        {
            r.Amenities.Should().NotBeEmpty();
            r.StarRating.Should().BeGreaterThan(0, because: "PremierStays always sets star ratings");
        });
    }

    [Fact]
    public async Task SearchAsync_SuiteRoom_HasHighestStarRatingAndRate()
    {
        var results = await _sut.SearchAsync(BuildCriteria());

        var suite = results.Single(r => r.RoomType == RoomType.Suite);
        var standard = results.Single(r => r.RoomType == RoomType.Standard);

        suite.StarRating.Should().BeGreaterThan(standard.StarRating ?? 0);
        suite.PerNightRate.Should().BeGreaterThan(standard.PerNightRate);
    }

    [Fact]
    public async Task SearchAsync_ProviderName_IsPremierStays()
    {
        var results = await _sut.SearchAsync(BuildCriteria());

        results.Should().AllSatisfy(r => r.Provider.Should().Be("PremierStays"));
    }
}
