using HotelStay.Application.DTOs.Auth;
using HotelStay.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelStay.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", RegisterAsync).AllowAnonymous();
        group.MapPost("/login", LoginAsync).AllowAnonymous();
        group.MapPost("/refresh", RefreshAsync).AllowAnonymous();
        group.MapPost("/logout", LogoutAsync).RequireAuthorization();
    }

    private static IResult RegisterAsync([FromBody] RegisterDto? dto, AuthService authService)
    {
        if (dto is null)
            return Results.Problem("Request body is required.", statusCode: 400, title: "Bad Request");

        if (string.IsNullOrWhiteSpace(dto.Email))
            return Results.Problem("Email is required.", statusCode: 400, title: "Bad Request");

        if (!dto.Email.Contains('@'))
            return Results.Problem("Invalid email format.", statusCode: 400, title: "Bad Request");

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
            return Results.Problem("Password must be at least 8 characters.", statusCode: 400, title: "Bad Request");

        var result = authService.Register(dto);
        return Results.Created("/auth/me", result);
    }

    private static IResult LoginAsync([FromBody] LoginDto? dto, AuthService authService)
    {
        if (dto is null)
            return Results.Problem("Request body is required.", statusCode: 400, title: "Bad Request");

        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return Results.Problem("Email and password are required.", statusCode: 400, title: "Bad Request");

        var result = authService.Login(dto);
        return Results.Ok(result);
    }

    private static IResult RefreshAsync([FromBody] RefreshTokenRequestDto? dto, AuthService authService)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.RefreshToken))
            return Results.Problem("Refresh token is required.", statusCode: 400, title: "Bad Request");

        var result = authService.Refresh(dto.RefreshToken);
        return Results.Ok(result);
    }

    private static IResult LogoutAsync([FromBody] RefreshTokenRequestDto? dto, AuthService authService)
    {
        if (dto is not null && !string.IsNullOrWhiteSpace(dto.RefreshToken))
            authService.Logout(dto.RefreshToken);

        return Results.NoContent();
    }
}
