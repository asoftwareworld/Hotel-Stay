namespace HotelStay.Domain.Exceptions;

public class DocumentMismatchException : Exception
{
    public string DestinationCity { get; }

    public DocumentMismatchException(string message, string destinationCity)
        : base(message)
    {
        DestinationCity = destinationCity;
    }
}
