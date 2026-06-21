using HotelStay.Domain.Entities;

namespace HotelStay.Application.Interfaces;

public interface IUserStore
{
    User? GetByEmail(string email);
    User? GetById(Guid id);
    void Save(User user);
    void SaveRefreshToken(RefreshToken token);
    RefreshToken? GetRefreshToken(string token);
    void RevokeRefreshToken(string token);
}
