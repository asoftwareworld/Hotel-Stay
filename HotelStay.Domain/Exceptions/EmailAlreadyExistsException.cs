namespace HotelStay.Domain.Exceptions;

public sealed class EmailAlreadyExistsException : Exception
{
    public EmailAlreadyExistsException(string email) : base($"Email '{email}' is already registered.") { }
}
