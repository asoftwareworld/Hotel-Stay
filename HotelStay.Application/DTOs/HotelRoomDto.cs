using HotelStay.Domain.Enums;

namespace HotelStay.Application.DTOs;

public record HotelRoomDto(
    string Provider,
    RoomType RoomType,
    decimal PerNightRate,
    decimal TotalPrice,
    int Nights,
    CancellationPolicy CancellationPolicy,
    string[]? Amenities,
    int? StarRating);
