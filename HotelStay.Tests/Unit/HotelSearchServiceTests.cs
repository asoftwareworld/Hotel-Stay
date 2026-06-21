using FluentAssertions;
using HotelStay.Application.DTOs;
using HotelStay.Application.Interfaces;
using HotelStay.Application.Services;
using HotelStay.Domain.Enums;
using HotelStay.Domain.Services;
using HotelStay.Domain.ValueObjects;
using Moq;
using Xunit;

namespace HotelStay.Tests.Unit;

public class HotelSearchServiceTests
{
    private static readonly DateOnly CheckIn = new(2025, 8, 1);
    private static readonly DateOnly CheckOut = new(2025, 8, 5);
    private static readonly CityClassificationService CityClassification = new();

    private static SearchQueryDto BuildQuery(RoomType? roomType = null) =>
        new("Paris", CheckIn, CheckOut, roomType);

    [Fact]
    public async Task SearchAsync_TwoProviders_MergesAndSortsResults()
    {
        var provider1 = new Mock<IHotelProvider>();
        provider1.Setup(p => p.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HotelRoom>
            {
                new("Provider1", RoomType.Standard, 100m, 400m, 4, CancellationPolicy.FreeCancellation, null, null),
            });

        var provider2 = new Mock<IHotelProvider>();
        provider2.Setup(p => p.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HotelRoom>
            {
                new("Provider2", RoomType.Deluxe, 150m, 600m, 4, CancellationPolicy.Flexible, null, null),
            });

        var sut = new HotelSearchService([provider1.Object, provider2.Object], CityClassification);

        var result = await sut.SearchAsync(BuildQuery());

        result.TotalResults.Should().Be(2);
        result.Results[0].TotalPrice.Should().BeLessOrEqualTo(result.Results[1].TotalPrice);
    }

    [Fact]
    public async Task SearchAsync_WithRoomTypeFilter_ReturnsOnlyMatchingRooms()
    {
        var provider = new Mock<IHotelProvider>();
        provider.Setup(p => p.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HotelRoom>
            {
                new("Provider1", RoomType.Standard, 100m, 400m, 4, CancellationPolicy.FreeCancellation, null, null),
                new("Provider1", RoomType.Deluxe,   150m, 600m, 4, CancellationPolicy.FreeCancellation, null, null),
                new("Provider1", RoomType.Suite,    200m, 800m, 4, CancellationPolicy.NonRefundable,    null, null),
            });

        var sut = new HotelSearchService([provider.Object], CityClassification);

        var result = await sut.SearchAsync(BuildQuery(RoomType.Standard));

        result.Results.Should().AllSatisfy(r => r.RoomType.Should().Be(RoomType.Standard));
        result.TotalResults.Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_ProviderReturnsEmpty_ReturnsEmptyResult()
    {
        var provider = new Mock<IHotelProvider>();
        provider.Setup(p => p.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HotelRoom>());

        var sut = new HotelSearchService([provider.Object], CityClassification);

        var result = await sut.SearchAsync(BuildQuery());

        result.Results.Should().BeEmpty();
        result.TotalResults.Should().Be(0);
    }
}
