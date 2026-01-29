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
            .GreaterThan(0).WithMessage("Artwork is required");

        RuleFor(x => x.StaffId)
            .GreaterThan(0).WithMessage("Staff member is required");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(512).WithMessage("Description must not exceed 512 characters")
            .When(x => x.Description != null);
    }
}

/// <summary>
/// Validator for UpdateRestorationDto.
/// </summary>
public class UpdateRestorationValidator : AbstractValidator<UpdateRestorationDto>
{
    public UpdateRestorationValidator()
    {
        RuleFor(x => x.ArtworkId)
            .GreaterThan(0).WithMessage("Artwork ID must be a positive number")
            .When(x => x.ArtworkId.HasValue);

        RuleFor(x => x.StaffId)
            .GreaterThan(0).WithMessage("Staff ID must be a positive number")
            .When(x => x.StaffId.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value).WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(512).WithMessage("Description must not exceed 512 characters")
            .When(x => x.Description != null);
    }
}
