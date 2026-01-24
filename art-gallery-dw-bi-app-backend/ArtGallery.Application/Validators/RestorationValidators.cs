using FluentValidation;
using ArtGallery.Application.DTOs.Restoration;

namespace ArtGallery.Application.Validators;

/// <summary>
/// Validator for CreateRestorationDto.
/// </summary>
public class CreateRestorationValidator : AbstractValidator<CreateRestorationDto>
{
    public CreateRestorationValidator()
    {
        RuleFor(x => x.ArtworkId)
            .GreaterThan(0).WithMessage("Artwork ID is required");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Restoration type is required")
            .MaximumLength(100).WithMessage("Type must not exceed 100 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.EstimatedCost)
            .GreaterThan(0).WithMessage("Estimated cost must be greater than 0")
            .When(x => x.EstimatedCost.HasValue);

        RuleFor(x => x.Conservator)
            .MaximumLength(255).WithMessage("Conservator name must not exceed 255 characters");
    }
}

/// <summary>
/// Validator for UpdateRestorationDto.
/// </summary>
public class UpdateRestorationValidator : AbstractValidator<UpdateRestorationDto>
{
    public UpdateRestorationValidator()
    {
        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.EstimatedCost)
            .GreaterThan(0).WithMessage("Estimated cost must be greater than 0")
            .When(x => x.EstimatedCost.HasValue);

        RuleFor(x => x.ActualCost)
            .GreaterThanOrEqualTo(0).WithMessage("Actual cost cannot be negative")
            .When(x => x.ActualCost.HasValue);
    }
}
