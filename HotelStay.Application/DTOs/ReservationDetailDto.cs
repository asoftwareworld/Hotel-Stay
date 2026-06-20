using HotelStay.Domain.Enums;

namespace HotelStay.Application.DTOs;

public record ReservationDetailDto(
    string Reference,
    string Provider,
    RoomType RoomType,
    string Destination,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Nights,
    decimal PerNightRate,
    decimal TotalPrice,
    CancellationPolicy CancellationPolicy,
    string GuestName,
    DocumentType DocumentType);
