namespace HotelStay.Application.DTOs.Auth;

public record TokenResponseDto(string AccessToken, string RefreshToken, int ExpiresIn);
