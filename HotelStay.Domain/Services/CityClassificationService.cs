using HotelStay.Domain.Enums;
using HotelStay.Domain.Exceptions;

namespace HotelStay.Domain.Services;

/// <summary>Classifies destinations as Domestic or International.</summary>
public class CityClassificationService
{
    private static readonly Dictionary<string, DestinationClass> _cities = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Oslo"]   = DestinationClass.Domestic,
        ["Bergen"] = DestinationClass.Domestic,
        ["Paris"]  = DestinationClass.International,
        ["London"] = DestinationClass.International,
        ["Tokyo"]  = DestinationClass.International,
    };

    /// <summary>Returns the destination class for a city name.</summary>
    /// <exception cref="UnknownDestinationException">Thrown when the city is not recognised.</exception>
    public DestinationClass Classify(string city)
    {
        if (_cities.TryGetValue(city, out var cls)) return cls;
        throw new UnknownDestinationException(city);
    }

    public IReadOnlyCollection<string> GetAllCities() => _cities.Keys.ToList().AsReadOnly();
}
