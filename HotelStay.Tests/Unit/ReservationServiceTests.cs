using FluentAssertions;
using HotelStay.Application.DTOs;
using HotelStay.Application.Interfaces;
using HotelStay.Application.Services;
using HotelStay.Domain.Entities;
using HotelStay.Domain.Enums;
using HotelStay.Domain.Exceptions;
using HotelStay.Domain.Services;
using Moq;
using Xunit;

namespace HotelStay.Tests.Unit;

public class ReservationServiceTests
{
    private readonly Mock<IReservationStore> _storeMock = new();
    private readonly CityClassificationService _cityClassification = new();
    private readonly DocumentValidationService _documentValidation = new();

    private ReservationService CreateSut() =>
        new(_storeMock.Object, _cityClassification, _documentValidation);

    private static ReserveRequestDto BuildRequest(
        string destination = "Oslo",
        DocumentType docType = DocumentType.NationalId) =>
        new(
            Provider: "PremierStays",
            RoomType: RoomType.Standard,
            Destination: destination,
            CheckIn: new DateOnly(2025, 9, 1),
            CheckOut: new DateOnly(2025, 9, 5),
            PerNightRate: 99m,
            CancellationPolicy: CancellationPolicy.FreeCancellation,
            GuestName: "John Doe",
            DocumentType: docType,
            DocumentNumber: "AB123456");

    [Fact]
    public async Task ReserveAsync_ValidRequest_ReturnsDetailDtoWithHsReference()
    {
        _storeMock.Setup(s => s.Save(It.IsAny<Reservation>()));

        var sut = CreateSut();

        var result = await sut.ReserveAsync(BuildRequest("Oslo", DocumentType.NationalId));

        result.Reference.Should().StartWith("HS-");
        result.Nights.Should().Be(4);
        result.TotalPrice.Should().Be(99m * 4);
    }

    [Fact]
    public async Task ReserveAsync_NationalIdForInternationalCity_ThrowsDocumentMismatchException()
    {
        var sut = CreateSut();

        var act = async () => await sut.ReserveAsync(BuildRequest("Paris", DocumentType.NationalId));

        await act.Should().ThrowAsync<DocumentMismatchException>()
            .WithMessage("*Paris*");
    }

    [Fact]
    public async Task GetByReferenceAsync_UnknownReference_ThrowsReservationNotFoundException()
    {
        _storeMock.Setup(s => s.GetByReference("HS-ZZZZZZ")).Returns((Reservation?)null);

        var sut = CreateSut();

        var act = async () => await sut.GetByReferenceAsync("HS-ZZZZZZ");

        await act.Should().ThrowAsync<ReservationNotFoundException>()
            .WithMessage("*HS-ZZZZZZ*");
    }
}
