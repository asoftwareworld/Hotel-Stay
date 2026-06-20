namespace HotelStay.Domain.Exceptions;

public class ReservationNotFoundException : Exception
{
    public ReservationNotFoundException(string reference)
        : base($"Reservation '{reference}' was not found.")
    {
    }
}
