using FluentValidation;
using HotelStay.Application.DTOs;
using HotelStay.Application.Services;
using HotelStay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelStay.Api.Controllers;

[ApiController]
[Route("hotels")]
[Authorize]
public class HotelsController : ControllerBase
{
    private readonly HotelSearchService _searchService;
    private readonly ReservationService _reservationService;
    private readonly IValidator<SearchQueryDto> _searchValidator;
    private readonly IValidator<ReserveRequestDto> _reserveValidator;

    public HotelsController(
        HotelSearchService searchService,
        ReservationService reservationService,
        IValidator<SearchQueryDto> searchValidator,
        IValidator<ReserveRequestDto> reserveValidator)
    {
        _searchService = searchService;
        _reservationService = reservationService;
        _searchValidator = searchValidator;
        _reserveValidator = reserveValidator;
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search(
        [FromQuery] string? destination,
        [FromQuery] string? checkIn,
        [FromQuery] string? checkOut,
        [FromQuery] string? roomType,
        CancellationToken ct)
    {
        if (!DateOnly.TryParse(checkIn, out var parsedCheckIn))
            parsedCheckIn = default;

        if (!DateOnly.TryParse(checkOut, out var parsedCheckOut))
            parsedCheckOut = default;

        RoomType? parsedRoomType = null;
        if (!string.IsNullOrWhiteSpace(roomType))
        {
            if (!Enum.TryParse<RoomType>(roomType, ignoreCase: true, out var rt))
                return Problem(
                    detail: $"Invalid roomType '{roomType}'. Valid values: Standard, Deluxe, Suite.",
                    title: "Bad Request",
                    statusCode: StatusCodes.Status400BadRequest);

            parsedRoomType = rt;
        }

        var query = new SearchQueryDto(destination ?? string.Empty, parsedCheckIn, parsedCheckOut, parsedRoomType);

        var validation = await _searchValidator.ValidateAsync(query, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var result = await _searchService.SearchAsync(query, ct);
        return Ok(result);
    }

    [HttpPost("reserve")]
    [ProducesResponseType(typeof(ReservationDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Reserve([FromBody] ReserveRequestDto request, CancellationToken ct)
    {
        var validation = await _reserveValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var result = await _reservationService.ReserveAsync(request, ct);
        return CreatedAtAction(nameof(GetReservation), new { reference = result.Reference }, result);
    }

    [HttpGet("reservation/{reference}")]
    [ProducesResponseType(typeof(ReservationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservation(string reference, CancellationToken ct)
    {
        var result = await _reservationService.GetByReferenceAsync(reference, ct);
        return Ok(result);
    }
}
