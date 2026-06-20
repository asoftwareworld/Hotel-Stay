using HotelStay.Domain.ValueObjects;

namespace HotelStay.Application.Interfaces;

/// <summary>Contract for all hotel data providers.</summary>
public interface IHotelProvider
{
    /// <summary>Human-readable name of this provider, included in search results.</summary>
    string ProviderName { get; }

    /// <summary>
    /// Returns available rooms matching the search criteria.
    /// Implementations filter unavailable rooms internally.
    /// Same criteria always produces the same result set (deterministic).
    /// </summary>
    Task<IReadOnlyList<HotelRoom>> SearchAsync(SearchCriteria criteria, CancellationToken ct = default);
}
