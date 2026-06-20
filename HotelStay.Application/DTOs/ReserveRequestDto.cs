using HotelStay.Domain.Enums;

namespace HotelStay.Application.DTOs;

public record ReserveRequestDto(
    string Provider,
    RoomType RoomType,
    string Destination,
    DateOnly CheckIn,
    DateOnly CheckOut,
    decimal PerNightRate,
    CancellationPolicy CancellationPolicy,
    string GuestName,
    DocumentType DocumentType,
    string DocumentNumber);
