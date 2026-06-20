using System.Text.Json;
using HotelStay.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace HotelStay.Api.Middleware;

public class ExceptionMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (DocumentMismatchException ex)
        {
            _logger.LogWarning("Document validation failed for destination {City}", ex.DestinationCity);
            await WriteProblemAsync(context, StatusCodes.Status422UnprocessableEntity,
                "Document Validation Failed", ex.Message);
        }
        catch (UnknownDestinationException ex)
        {
            _logger.LogWarning("Unknown destination requested: {Message}", ex.Message);
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest,
                "Bad Request", ex.Message);
        }
        catch (ReservationNotFoundException ex)
        {
            _logger.LogInformation("Reservation not found: {Message}", ex.Message);
            await WriteProblemAsync(context, StatusCodes.Status404NotFound,
                "Not Found", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred");
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError,
                "Internal Server Error", "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        await context.Response.WriteAsync(json);
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}
