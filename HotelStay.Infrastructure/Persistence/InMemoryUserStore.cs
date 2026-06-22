using System.Collections.Concurrent;
using HotelStay.Application.Interfaces;
using HotelStay.Domain.Entities;

namespace HotelStay.Infrastructure.Persistence;

public sealed class InMemoryUserStore : IUserStore
{
    private static readonly User DefaultUser = new(
        Id: new Guid("00000000-0000-0000-0000-000000000001"),
        Email: "admin@hotelstay.com",
        Username: "admin",
        PasswordHash: "$2a$12$8ISkHmcZYKCl/v66PzmeF.naTrLD0VbpehJKIwsYKDAMQhBSimuSa", // Admin1234
        Role: UserRole.Admin,
        CreatedAt: DateTimeOffset.UtcNow);

    private readonly ConcurrentDictionary<string, User> _byEmail = new(StringComparer.OrdinalIgnoreCase)
    {
        [DefaultUser.Email] = DefaultUser,
    };
    private readonly ConcurrentDictionary<Guid, User> _byId = new()
    {
        [DefaultUser.Id] = DefaultUser,
    };
    private readonly ConcurrentDictionary<string, RefreshToken> _tokens = new();

    public User? GetByEmail(string email)
        => _byEmail.TryGetValue(email, out var user) ? user : null;

    public User? GetById(Guid id)
        => _byId.TryGetValue(id, out var user) ? user : null;

    public void Save(User user)
    {
        _byEmail[user.Email] = user;
        _byId[user.Id] = user;
    }

    public void SaveRefreshToken(RefreshToken token)
        => _tokens[token.Token] = token;

    public RefreshToken? GetRefreshToken(string token)
        => _tokens.TryGetValue(token, out var t) ? t : null;

    public void RevokeRefreshToken(string token)
    {
        if (_tokens.TryGetValue(token, out var existing))
            _tokens[token] = existing with { RevokedAt = DateTimeOffset.UtcNow };
    }
}
