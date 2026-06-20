using FluentAssertions;
using HotelStay.Domain.Enums;
using HotelStay.Domain.Exceptions;
using HotelStay.Domain.Services;
using Xunit;

namespace HotelStay.Tests.Unit;

public class CityClassificationServiceTests
{
    private readonly CityClassificationService _sut = new();

    [Fact]
    public void Classify_Oslo_ReturnsDomestic()
    {
        var result = _sut.Classify("Oslo");

        result.Should().Be(DestinationClass.Domestic);
    }

    [Fact]
    public void Classify_Paris_ReturnsInternational()
    {
        var result = _sut.Classify("Paris");

        result.Should().Be(DestinationClass.International);
    }

    [Fact]
    public void Classify_LowerCaseOslo_ReturnsDomestic_CaseInsensitive()
    {
        var result = _sut.Classify("oslo");

        result.Should().Be(DestinationClass.Domestic);
    }

    [Fact]
    public void Classify_UnknownCity_ThrowsUnknownDestinationException()
    {
        var act = () => _sut.Classify("Atlantis");

        act.Should().Throw<UnknownDestinationException>()
            .WithMessage("*Atlantis*");
    }
}
