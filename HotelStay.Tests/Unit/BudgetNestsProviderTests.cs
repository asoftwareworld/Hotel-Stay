using FluentAssertions;
using HotelStay.Domain.Enums;
using HotelStay.Domain.ValueObjects;
using HotelStay.Infrastructure.Providers;
using Xunit;

namespace HotelStay.Tests.Unit;

public class BudgetNestsProviderTests
{
    private readonly BudgetNestsProvider _sut = new();

    private static SearchCriteria BuildCriteria(RoomType? roomType = null, int nights = 3) =>
        new(
            Destination: "Oslo",
            CheckIn: new DateOnly(2025, 10, 1),
            CheckOut: new DateOnly(2025, 10, 1).AddDays(nights),
            RoomType: roomType);

    [Fact]
    public async Task SearchAsync_ReturnsOnlyAvailableRooms_SuiteFilteredOut()
    {
        var results = await _sut.SearchAsync(BuildCriteria());

        // 4 entries minus 1 unavailable Suite = 3
        results.Should().HaveCount(3);
        results.Should().NotContain(r => r.RoomType == RoomType.Suite);
    }

    [Fact]
    public async Task SearchAsync_WithStandardFilter_ReturnsOnlyStandardRooms()
    {
        var results = await _sut.SearchAsync(BuildCriteria(RoomType.Standard));

        results.Should().NotBeEmpty();
        results.Should().AllSatisfy(r => r.RoomType.Should().Be(RoomType.Standard));
    }

    [Fact]
    public async Task SearchAsync_TotalPrice_ComputedCorrectly()
    {
        var nights = 5;
        var results = await _sut.SearchAsync(BuildCriteria(nights: nights));

        results.Should().AllSatisfy(r =>
            r.TotalPrice.Should().Be(r.PerNightRate * nights));
    }
}
