using FluentValidation;
using ArtGallery.Application.DTOs.Insurance;

namespace ArtGallery.Application.Validators;

/// <summary>
/// Validator for CreateInsuranceDto.
/// </summary>
public class CreateInsuranceValidator : AbstractValidator<CreateInsuranceDto>
{
    public CreateInsuranceValidator()
    {
        RuleFor(x => x.ArtworkId)
            .GreaterThan(0).WithMessage("Artwork ID is required");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required")
            .MaximumLength(255).WithMessage("Provider must not exceed 255 characters");

        RuleFor(x => x.PolicyNumber)
            .NotEmpty().WithMessage("Policy number is required")
            .MaximumLength(100).WithMessage("Policy number must not exceed 100 characters");

        RuleFor(x => x.CoverageAmount)
            .GreaterThan(0).WithMessage("Coverage amount must be greater than 0");

        RuleFor(x => x.Premium)
            .GreaterThanOrEqualTo(0).WithMessage("Premium cannot be negative");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
    }
}

/// <summary>
/// Validator for UpdateInsuranceDto.
/// </summary>
public class UpdateInsuranceValidator : AbstractValidator<UpdateInsuranceDto>
{
    public UpdateInsuranceValidator()
    {
        RuleFor(x => x.CoverageAmount)
            .GreaterThan(0).WithMessage("Coverage amount must be greater than 0")
            .When(x => x.CoverageAmount.HasValue);

        RuleFor(x => x.Premium)
            .GreaterThanOrEqualTo(0).WithMessage("Premium cannot be negative")
            .When(x => x.Premium.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}
