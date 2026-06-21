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
            .WithTags("Health");

        app.MapGet("/hotels/search", SearchHotelsAsync)
            .WithName("SearchHotels")
            .WithTags("Hotels")
            .Produces<SearchResultDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        app.MapPost("/hotels/reserve", ReserveRoomAsync)
            .WithName("ReserveRoom")
            .WithTags("Hotels")
            .Produces<ReservationDetailDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        app.MapGet("/hotels/reservation/{reference}", GetReservationAsync)
            .WithName("GetReservation")
            .WithTags("Hotels")
            .Produces<ReservationDetailDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> SearchHotelsAsync(
        [FromQuery] string? destination,
        [FromQuery] string? checkIn,
        [FromQuery] string? checkOut,
        [FromQuery] string? roomType,
        HotelSearchService searchService,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            return Results.Problem(
                detail: "The 'destination' query parameter is required.",
                title: "Bad Request",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(checkIn) || !DateOnly.TryParse(checkIn, out var parsedCheckIn))
        {
            return Results.Problem(
                detail: "The 'checkIn' query parameter is required and must be a valid date (yyyy-MM-dd).",
                title: "Bad Request",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(checkOut) || !DateOnly.TryParse(checkOut, out var parsedCheckOut))
        {
            return Results.Problem(
                detail: "The 'checkOut' query parameter is required and must be a valid date (yyyy-MM-dd).",
                title: "Bad Request",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (parsedCheckOut <= parsedCheckIn)
        {
            return Results.Problem(
                detail: "The 'checkOut' date must be after the 'checkIn' date.",
                title: "Bad Request",
                statusCode: StatusCodes.Status400BadRequest);
        }

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

        var query = new SearchQueryDto(destination, parsedCheckIn, parsedCheckOut, parsedRoomType);
        var result = await searchService.SearchAsync(query, ct);

        return Results.Ok(result);
    }

    private static async Task<IResult> ReserveRoomAsync(
        [FromBody] ReserveRequestDto? request,
        ReservationService reservationService,
        CancellationToken ct)
    {
        if (request is null)
        {
            return Results.Problem(
                detail: "Request body is required.",
                title: "Bad Request",
                statusCode: StatusCodes.Status400BadRequest);
        }

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
