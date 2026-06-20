using HotelStay.Domain.Enums;

namespace HotelStay.Domain.ValueObjects;

public record SearchCriteria(
    string Destination,
    DateOnly CheckIn,
    DateOnly CheckOut,
    RoomType? RoomType = null)
{
    public int Nights => CheckOut.DayNumber - CheckIn.DayNumber;
}
