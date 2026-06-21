namespace HotelStay.Domain.Entities;

public record RefreshToken(
    string Token,
    Guid UserId,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt = null)
{
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;
}
