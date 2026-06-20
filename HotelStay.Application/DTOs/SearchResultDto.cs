namespace HotelStay.Application.DTOs;

public record SearchResultDto(
    IReadOnlyList<HotelRoomDto> Results,
    string Destination,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int TotalResults);
