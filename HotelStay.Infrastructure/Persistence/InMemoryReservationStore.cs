using System.Collections.Concurrent;
using HotelStay.Application.Interfaces;
using HotelStay.Domain.Entities;

namespace HotelStay.Infrastructure.Persistence;

/// <summary>Thread-safe in-memory reservation store backed by ConcurrentDictionary.</summary>
public sealed class InMemoryReservationStore : IReservationStore
{
    private readonly ConcurrentDictionary<string, Reservation> _store = new(StringComparer.OrdinalIgnoreCase);

    public void Save(Reservation reservation)
    {
        if (!_store.TryAdd(reservation.Reference, reservation))
            throw new InvalidOperationException($"Reservation with reference '{reservation.Reference}' already exists.");
    }

    public Reservation? GetByReference(string reference) =>
        _store.TryGetValue(reference, out var r) ? r : null;
}
