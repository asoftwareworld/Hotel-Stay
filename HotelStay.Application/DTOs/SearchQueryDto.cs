using HotelStay.Domain.Enums;

namespace HotelStay.Application.DTOs;

public record SearchQueryDto(string Destination, DateOnly CheckIn, DateOnly CheckOut, RoomType? RoomType = null);
