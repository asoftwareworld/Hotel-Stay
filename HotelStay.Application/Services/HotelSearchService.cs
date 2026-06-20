using HotelStay.Application.DTOs;
using HotelStay.Application.Interfaces;
using HotelStay.Domain.ValueObjects;

namespace HotelStay.Application.Services;

public class HotelSearchService
{
    private readonly IEnumerable<IHotelProvider> _providers;

    public HotelSearchService(IEnumerable<IHotelProvider> providers)
    {
        _providers = providers;
    }

    public async Task<SearchResultDto> SearchAsync(SearchQueryDto query, CancellationToken ct = default)
    {
        var criteria = new SearchCriteria(
            query.Destination,
            query.CheckIn,
            query.CheckOut,
            query.RoomType);

        var providerTasks = _providers.Select(p => p.SearchAsync(criteria, ct));
        var providerResults = await Task.WhenAll(providerTasks);

        var allRooms = providerResults
            .SelectMany(rooms => rooms);

        if (query.RoomType.HasValue)
        {
            allRooms = allRooms.Where(r => r.RoomType == query.RoomType.Value);
        }

        var sorted = allRooms
            .OrderBy(r => r.TotalPrice)
            .ToList();

        var dtos = sorted.Select(r => new HotelRoomDto(
            r.Provider,
            r.RoomType,
            r.PerNightRate,
            r.TotalPrice,
            r.Nights,
            r.CancellationPolicy,
            r.Amenities,
            r.StarRating))
            .ToList();

        return new SearchResultDto(
            dtos,
            query.Destination,
            query.CheckIn,
            query.CheckOut,
            dtos.Count);
    }
}
