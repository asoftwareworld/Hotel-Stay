using FluentValidation;
using HotelStay.Application.DTOs;

namespace HotelStay.Api.Validators;

/// <summary>Validates query parameters for the hotel search endpoint.</summary>
public sealed class SearchQueryValidator : AbstractValidator<SearchQueryDto>
{
    public SearchQueryValidator()
    {
        RuleFor(x => x.Destination)
            .NotEmpty()
            .WithMessage("The 'destination' query parameter is required.");

        RuleFor(x => x.CheckIn)
            .NotEmpty()
            .WithMessage("The 'checkIn' query parameter is required and must be a valid date (yyyy-MM-dd).");

        RuleFor(x => x.CheckOut)
            .NotEmpty()
            .WithMessage("The 'checkOut' query parameter is required and must be a valid date (yyyy-MM-dd).");

        RuleFor(x => x)
            .Must(q => q.CheckOut > q.CheckIn)
            .WithName("checkOut")
            .WithMessage("The 'checkOut' date must be after the 'checkIn' date.")
            .When(q => q.CheckIn != default && q.CheckOut != default);
    }
}
