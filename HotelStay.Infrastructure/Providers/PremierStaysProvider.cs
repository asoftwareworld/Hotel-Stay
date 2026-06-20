using HotelStay.Application.Interfaces;
using HotelStay.Domain.Enums;
using HotelStay.Domain.ValueObjects;

namespace HotelStay.Infrastructure.Providers;

public sealed class PremierStaysProvider : IHotelProvider
{
    public string ProviderName => "PremierStays";

    private static readonly IReadOnlyList<(RoomType RoomType, decimal Rate, CancellationPolicy Policy, int Stars, string[] Amenities)> _catalog =
    [
        (RoomType.Standard,  99m,  CancellationPolicy.FreeCancellation, 3, ["WiFi", "TV"]),
        (RoomType.Deluxe,   149m,  CancellationPolicy.FreeCancellation, 4, ["WiFi", "Breakfast", "Pool"]),
        (RoomType.Suite,    229m,  CancellationPolicy.NonRefundable,    5, ["WiFi", "Breakfast", "Pool", "Spa", "Concierge"]),
    ];

    public Task<IReadOnlyList<HotelRoom>> SearchAsync(SearchCriteria criteria, CancellationToken ct = default)
    {
        var nights = criteria.Nights;

        var query = _catalog.AsEnumerable();

        if (criteria.RoomType.HasValue)
        {
            query = query.Where(r => r.RoomType == criteria.RoomType.Value);
        }

        IReadOnlyList<HotelRoom> results = query
            .Select(r => new HotelRoom(
                Provider: ProviderName,
                RoomType: r.RoomType,
                PerNightRate: r.Rate,
                TotalPrice: r.Rate * nights,
                Nights: nights,
                CancellationPolicy: r.Policy,
                Amenities: r.Amenities,
                StarRating: r.Stars))
            .ToList();

        return Task.FromResult(results);
    }
}
