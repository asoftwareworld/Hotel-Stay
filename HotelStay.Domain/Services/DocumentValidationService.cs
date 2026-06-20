using HotelStay.Domain.Enums;
using HotelStay.Domain.Exceptions;

namespace HotelStay.Domain.Services;

/// <summary>Validates document type against destination classification.</summary>
public class DocumentValidationService
{
    /// <summary>
    /// Validates that the provided document type is acceptable for the destination.
    /// International destinations require a Passport.
    /// </summary>
    /// <exception cref="DocumentMismatchException">Thrown when document type is invalid for the destination.</exception>
    public void Validate(string destinationCity, DestinationClass destinationClass, DocumentType documentType)
    {
        if (destinationClass == DestinationClass.International && documentType == DocumentType.NationalId)
        {
            throw new DocumentMismatchException(
                $"International destination '{destinationCity}' requires a Passport. National ID is not accepted.",
                destinationCity);
        }
    }
}
