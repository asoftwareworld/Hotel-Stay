using FluentAssertions;
using HotelStay.Domain.Enums;
using HotelStay.Domain.Exceptions;
using HotelStay.Domain.Services;
using Xunit;

namespace HotelStay.Tests.Unit;

public class DocumentValidationServiceTests
{
    private readonly DocumentValidationService _sut = new();

    [Fact]
    public void Validate_InternationalDestination_WithPassport_DoesNotThrow()
    {
        var act = () => _sut.Validate("Paris", DestinationClass.International, DocumentType.Passport);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_InternationalDestination_WithNationalId_ThrowsDocumentMismatchException()
    {
        var act = () => _sut.Validate("Paris", DestinationClass.International, DocumentType.NationalId);

        act.Should().Throw<DocumentMismatchException>()
            .WithMessage("*Paris*")
            .Which.DestinationCity.Should().Be("Paris");
    }

    [Fact]
    public void Validate_DomesticDestination_WithNationalId_DoesNotThrow()
    {
        var act = () => _sut.Validate("Oslo", DestinationClass.Domestic, DocumentType.NationalId);

        act.Should().NotThrow();
    }
}
