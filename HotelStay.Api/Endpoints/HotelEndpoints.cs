using FluentValidation;
using HotelStay.Application.DTOs;
using HotelStay.Application.Services;
using HotelStay.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HotelStay.Api.Endpoints;

public static class HotelEndpoints
{
    public static void MapHotelEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Ok(new { status = "ok", service = "HotelStay API", version = "1.0" }))
            .AllowAnonymous()
            .WithTags("Health");

        app.MapGet("/hotels/search", SearchHotelsAsync)
            .WithName("SearchHotels")
            .WithTags("Hotels")
            .RequireAuthorization()
            .Produces<SearchResultDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        app.MapPost("/hotels/reserve", ReserveRoomAsync)
            .WithName("ReserveRoom")
            .WithTags("Hotels")
            .RequireAuthorization()
            .Produces<ReservationDetailDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        app.MapGet("/hotels/reservation/{reference}", GetReservationAsync)
            .WithName("GetReservation")
            .WithTags("Hotels")
            .RequireAuthorization()
            .Produces<ReservationDetailDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> SearchHotelsAsync(
        [FromQuery] string? destination,
        [FromQuery] string? checkIn,
        [FromQuery] string? checkOut,
        [FromQuery] string? roomType,
        HotelSearchService searchService,
        IValidator<SearchQueryDto> validator,
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
            {
                return Results.Problem(
                    detail: $"Invalid roomType '{roomType}'. Valid values: Standard, Deluxe, Suite.",
                    title: "Bad Request",
                    statusCode: StatusCodes.Status400BadRequest);
            }
            parsedRoomType = rt;
        }

        var query = new SearchQueryDto(destination ?? string.Empty, parsedCheckIn, parsedCheckOut, parsedRoomType);

        var validation = await validator.ValidateAsync(query, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var result = await searchService.SearchAsync(query, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> ReserveRoomAsync(
        [FromBody] ReserveRequestDto? request,
        ReservationService reservationService,
        IValidator<ReserveRequestDto> validator,
        CancellationToken ct)
    {
        if (request is null)
            return Results.Problem("Request body is required.", statusCode: 400, title: "Bad Request");

        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var result = await reservationService.ReserveAsync(request, ct);
        return Results.Created($"/hotels/reservation/{result.Reference}", result);
    }

    private static async Task<IResult> GetReservationAsync(
        string reference,
        ReservationService reservationService,
        CancellationToken ct)
    {
        var result = await reservationService.GetByReferenceAsync(reference, ct);
        return Results.Ok(result);
    }
}
