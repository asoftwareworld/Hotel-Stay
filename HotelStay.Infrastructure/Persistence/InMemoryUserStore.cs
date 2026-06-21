using System.Collections.Concurrent;
using HotelStay.Application.Interfaces;
using HotelStay.Domain.Entities;

namespace HotelStay.Infrastructure.Persistence;

public sealed class InMemoryUserStore : IUserStore
{
    private readonly ConcurrentDictionary<string, User> _byEmail = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, User> _byId = new();
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
