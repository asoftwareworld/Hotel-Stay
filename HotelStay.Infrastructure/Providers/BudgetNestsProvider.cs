using HotelStay.Application.Interfaces;
using HotelStay.Domain.Enums;
using HotelStay.Domain.ValueObjects;

namespace HotelStay.Infrastructure.Providers;

public sealed class BudgetNestsProvider : IHotelProvider
{
    public string ProviderName => "BudgetNests";

    private record StubEntry(RoomType RoomType, decimal Rate, CancellationPolicy Policy, bool Available);

    private static readonly IReadOnlyList<StubEntry> _catalog =
    [
        new StubEntry(RoomType.Standard,  59m, CancellationPolicy.NonRefundable, Available: true),
        new StubEntry(RoomType.Deluxe,    89m, CancellationPolicy.Flexible,      Available: true),
        new StubEntry(RoomType.Suite,    129m, CancellationPolicy.Flexible,      Available: false),
        new StubEntry(RoomType.Standard,  45m, CancellationPolicy.NonRefundable, Available: true),
    ];

    public Task<IReadOnlyList<HotelRoom>> SearchAsync(SearchCriteria criteria, CancellationToken ct = default)
    {
        var nights = criteria.Nights;

        var query = _catalog
            .Where(r => r.Available);

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
                Amenities: null,
                StarRating: null))
            .ToList();

        return Task.FromResult(results);
    }
}
