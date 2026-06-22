using HotelStay.Application.DTOs.Auth;
using HotelStay.Application.Interfaces;
using HotelStay.Domain.Entities;
using HotelStay.Domain.Exceptions;

namespace HotelStay.Application.Services;

public class AuthService
{
    private readonly IUserStore _userStore;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUserStore userStore, IJwtService jwtService, IPasswordHasher passwordHasher)
    {
        _userStore = userStore;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    public TokenResponseDto Register(RegisterDto dto)
    {
        if (_userStore.GetByEmail(dto.Email.ToLowerInvariant()) is not null)
            throw new EmailAlreadyExistsException(dto.Email);

        var user = new User(
            Guid.NewGuid(),
            dto.Email.ToLowerInvariant(),
            dto.Username.Trim(),
            _passwordHasher.Hash(dto.Password),
            UserRole.User,
            DateTimeOffset.UtcNow);

        _userStore.Save(user);
        return IssueTokens(user);
    }

    public TokenResponseDto Login(LoginDto dto)
    {
        var user = _userStore.GetByEmail(dto.Email.ToLowerInvariant())
            ?? throw new InvalidCredentialsException();

        if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        return IssueTokens(user);
    }

    public TokenResponseDto Refresh(string refreshToken)
    {
        var stored = _userStore.GetRefreshToken(refreshToken)
            ?? throw new InvalidRefreshTokenException();

        if (!stored.IsActive)
            throw new InvalidRefreshTokenException();

        var user = _userStore.GetById(stored.UserId)
            ?? throw new InvalidRefreshTokenException();

        // Rotate: revoke the consumed token, issue a fresh pair
        _userStore.RevokeRefreshToken(refreshToken);
        return IssueTokens(user);
    }

    public void Logout(string refreshToken)
    {
        _userStore.RevokeRefreshToken(refreshToken);
    }

    private TokenResponseDto IssueTokens(User user)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        _userStore.SaveRefreshToken(new RefreshToken(
            newRefreshToken,
            user.Id,
            DateTimeOffset.UtcNow.AddDays(TokenExpiryConstants.RefreshTokenExpiryDays)));

        return new TokenResponseDto(
            accessToken,
            newRefreshToken,
            TokenExpiryConstants.AccessTokenExpiryMinutes * 60);
    }
}

// Shared constants so Application layer doesn't depend on IConfiguration
public static class TokenExpiryConstants
{
    public const int AccessTokenExpiryMinutes = 15;
    public const int RefreshTokenExpiryDays = 7;
}
