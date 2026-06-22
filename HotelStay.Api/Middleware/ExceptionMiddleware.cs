using System.Text.Json;
using FluentValidation;
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
        catch (ValidationException ex)
        {
            var detail = string.Join(" ", ex.Errors.Select(e => e.ErrorMessage));
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Bad Request", detail);
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
        catch (HotelStay.Domain.Exceptions.InvalidCredentialsException ex)
        {
            _logger.LogInformation("Login failed: {Message}", ex.Message);
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized,
                "Unauthorized", ex.Message);
        }
        catch (HotelStay.Domain.Exceptions.EmailAlreadyExistsException ex)
        {
            _logger.LogInformation("Registration conflict: {Message}", ex.Message);
            await WriteProblemAsync(context, StatusCodes.Status409Conflict,
                "Conflict", ex.Message);
        }
        catch (HotelStay.Domain.Exceptions.InvalidRefreshTokenException ex)
        {
            _logger.LogInformation("Refresh token invalid: {Message}", ex.Message);
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized,
                "Unauthorized", ex.Message);
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
        if (context.Response.HasStarted)
        {
            // Headers already sent — cannot write a new error response; abort silently.
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
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
