using HotelStay.Application.DTOs.Auth;
using HotelStay.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelStay.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public IActionResult Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return Problem("Email is required.", statusCode: 400, title: "Bad Request");

        if (!dto.Email.Contains('@'))
            return Problem("Invalid email format.", statusCode: 400, title: "Bad Request");

        if (string.IsNullOrWhiteSpace(dto.Username))
            return Problem("Username is required.", statusCode: 400, title: "Bad Request");

        if (dto.Username.Trim().Length < 2)
            return Problem("Username must be at least 2 characters.", statusCode: 400, title: "Bad Request");

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
            return Problem("Password must be at least 8 characters.", statusCode: 400, title: "Bad Request");

        var result = _authService.Register(dto);
        return Created("/auth/me", result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return Problem("Email and password are required.", statusCode: 400, title: "Bad Request");

        var result = _authService.Login(dto);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            return Problem("Refresh token is required.", statusCode: 400, title: "Bad Request");

        var result = _authService.Refresh(dto.RefreshToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout([FromBody] RefreshTokenRequestDto? dto)
    {
        if (dto is not null && !string.IsNullOrWhiteSpace(dto.RefreshToken))
            _authService.Logout(dto.RefreshToken);

        return NoContent();
    }
}
