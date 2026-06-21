using HotelStay.Application.Interfaces;
using HotelStay.Infrastructure.Auth;
using HotelStay.Infrastructure.Persistence;
using HotelStay.Infrastructure.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace HotelStay.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IHotelProvider, PremierStaysProvider>();
        services.AddSingleton<IHotelProvider, BudgetNestsProvider>();
        services.AddSingleton<IReservationStore, InMemoryReservationStore>();
        services.AddSingleton<IUserStore, InMemoryUserStore>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        return services;
    }
}
