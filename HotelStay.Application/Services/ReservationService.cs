using HotelStay.Application.DTOs;
using HotelStay.Application.Interfaces;
using HotelStay.Domain.Entities;
using HotelStay.Domain.Exceptions;
using HotelStay.Domain.Services;
using Microsoft.Extensions.Logging;

namespace HotelStay.Application.Services;

public class ReservationService
{
    private readonly IReservationStore _store;
    private readonly CityClassificationService _cityClassification;
    private readonly DocumentValidationService _documentValidation;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(
        IReservationStore store,
        CityClassificationService cityClassification,
        DocumentValidationService documentValidation,
        ILogger<ReservationService> logger)
    {
        _store = store;
        _cityClassification = cityClassification;
        _documentValidation = documentValidation;
        _logger = logger;
    }

    public async Task<ReservationDetailDto> ReserveAsync(ReserveRequestDto request, CancellationToken ct = default)
    {
        var destinationClass = _cityClassification.Classify(request.Destination);
        _documentValidation.Validate(request.Destination, destinationClass, request.DocumentType);

        var reference = "HS-" + Guid.NewGuid().ToString("N")[..6].ToUpper();
        var nights = request.CheckOut.DayNumber - request.CheckIn.DayNumber;
        var totalPrice = request.PerNightRate * nights;

        var reservation = new Reservation(
            Reference: reference,
            Provider: request.Provider,
            RoomType: request.RoomType,
            Destination: request.Destination,
            CheckIn: request.CheckIn,
            CheckOut: request.CheckOut,
            GuestName: request.GuestName,
            DocumentType: request.DocumentType,
            DocumentNumber: request.DocumentNumber,
            PerNightRate: request.PerNightRate,
            TotalPrice: totalPrice,
            Nights: nights,
            CancellationPolicy: request.CancellationPolicy,
            CreatedAt: DateTimeOffset.UtcNow);

        _store.Save(reservation);

        _logger.LogInformation(
            "Reservation {Reference} created for guest {GuestName} at {Destination} ({Nights} nights). DocumentNumber: [REDACTED]",
            reference, request.GuestName, request.Destination, nights);

        await Task.CompletedTask;

        return MapToDto(reservation);
    }

    public async Task<ReservationDetailDto> GetByReferenceAsync(string reference, CancellationToken ct = default)
    {
        var reservation = _store.GetByReference(reference);

        if (reservation is null)
        {
            throw new ReservationNotFoundException(reference);
        }

        await Task.CompletedTask;

        return MapToDto(reservation);
    }

    private static ReservationDetailDto MapToDto(Reservation r) =>
        new(
            r.Reference,
            r.Provider,
            r.RoomType,
            r.Destination,
            r.CheckIn,
            r.CheckOut,
            r.Nights,
            r.PerNightRate,
            r.TotalPrice,
            r.CancellationPolicy,
            r.GuestName,
            r.DocumentType);
}
