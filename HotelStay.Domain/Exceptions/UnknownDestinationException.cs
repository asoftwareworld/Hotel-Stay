namespace HotelStay.Domain.Exceptions;

public class UnknownDestinationException : Exception
{
    public UnknownDestinationException(string city)
        : base($"Unknown destination: '{city}'.")
    {
    }
}
