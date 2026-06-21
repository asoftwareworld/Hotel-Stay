namespace HotelStay.Domain.Exceptions;

public sealed class InvalidRefreshTokenException : Exception
{
    public InvalidRefreshTokenException() : base("The refresh token is invalid or has expired.") { }
}
