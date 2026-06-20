using HotelStay.Domain.Entities;

namespace HotelStay.Application.Interfaces;

/// <summary>In-memory reservation persistence contract.</summary>
public interface IReservationStore
{
    /// <summary>Persists a new reservation.</summary>
    void Save(Reservation reservation);

    /// <summary>Returns the reservation with the given reference, or null if not found.</summary>
    Reservation? GetByReference(string reference);
}
