using FluentValidation;
using HotelStay.Application.DTOs;

namespace HotelStay.Api.Validators;

/// <summary>Validates the request body for the room reservation endpoint.</summary>
public sealed class ReserveRequestValidator : AbstractValidator<ReserveRequestDto>
{
    public ReserveRequestValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage("The 'provider' field is required.");

        RuleFor(x => x.Destination)
            .NotEmpty()
            .WithMessage("The 'destination' field is required.");

        RuleFor(x => x.GuestName)
            .NotEmpty()
            .WithMessage("The 'guestName' field is required.");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty()
            .WithMessage("The 'documentNumber' field is required.");

        RuleFor(x => x.PerNightRate)
            .GreaterThan(0)
            .WithMessage("The 'perNightRate' must be greater than zero.");

        RuleFor(x => x)
            .Must(r => r.CheckOut > r.CheckIn)
            .WithName("checkOut")
            .WithMessage("The 'checkOut' date must be after the 'checkIn' date.")
            .When(r => r.CheckIn != default && r.CheckOut != default);
    }
}
