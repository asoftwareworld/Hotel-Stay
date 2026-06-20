using HotelStay.Domain.Enums;

namespace HotelStay.Domain.Entities;

public record Reservation(
    string Reference,
    string Provider,
    RoomType RoomType,
    string Destination,
    DateOnly CheckIn,
    DateOnly CheckOut,
    string GuestName,
    DocumentType DocumentType,
    string DocumentNumber,
    decimal PerNightRate,
    decimal TotalPrice,
    int Nights,
    CancellationPolicy CancellationPolicy,
    DateTimeOffset CreatedAt);
