using HotelStay.Domain.Entities;

namespace HotelStay.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
