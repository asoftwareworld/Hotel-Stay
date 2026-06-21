using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using HotelStay.Api.Endpoints;
using HotelStay.Api.Middleware;
using HotelStay.Application.Services;
using HotelStay.Domain.Services;
using HotelStay.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) =>
    {
        config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .WriteTo.Console();
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngular", policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // JWT authentication
    var jwtKey = builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("Jwt:Key is not configured.");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero,
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddInfrastructure();

    builder.Services.AddSingleton<CityClassificationService>();
    builder.Services.AddSingleton<DocumentValidationService>();

    builder.Services.AddScoped<HotelSearchService>();
    builder.Services.AddScoped<ReservationService>();
    builder.Services.AddScoped<AuthService>();

    builder.Services.AddTransient<ExceptionMiddleware>();

    builder.Services.AddProblemDetails();

    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            });
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    var app = builder.Build();

    // Serilog outermost so it sees the final status code after exception handling
    app.UseSerilogRequestLogging();
    app.UseExceptionMiddleware();
    app.UseCors("AllowAngular");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapAuthEndpoints();
    app.MapHotelEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
