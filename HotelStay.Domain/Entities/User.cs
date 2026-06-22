namespace HotelStay.Domain.Entities;

public record User(
    Guid Id,
    string Email,
    string Username,
    string PasswordHash,
    UserRole Role,
    DateTimeOffset CreatedAt);

public enum UserRole { User, Admin }
