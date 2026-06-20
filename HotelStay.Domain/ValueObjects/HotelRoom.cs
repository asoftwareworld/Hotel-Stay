using HotelStay.Domain.Enums;

namespace HotelStay.Domain.ValueObjects;

public record HotelRoom(
    string Provider,
    RoomType RoomType,
    decimal PerNightRate,
    decimal TotalPrice,
    int Nights,
    CancellationPolicy CancellationPolicy,
    string[]? Amenities,
    int? StarRating);
