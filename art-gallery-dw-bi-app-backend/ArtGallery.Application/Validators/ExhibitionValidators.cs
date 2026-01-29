using FluentValidation;
using ArtGallery.Application.DTOs.Exhibition;

namespace ArtGallery.Application.Validators;

/// <summary>
/// Validator for CreateExhibitionDto.
/// </summary>
public class CreateExhibitionValidator : AbstractValidator<CreateExhibitionDto>
{
    public CreateExhibitionValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(128).WithMessage("Title must not exceed 128 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.ExhibitorId)
            .GreaterThan(0).WithMessage("Exhibitor is required");

        RuleFor(x => x.Description)
            .MaximumLength(512).WithMessage("Description must not exceed 512 characters")
            .When(x => x.Description != null);
    }
}

/// <summary>
/// Validator for UpdateExhibitionDto.
/// </summary>
public class UpdateExhibitionValidator : AbstractValidator<UpdateExhibitionDto>
{
    public UpdateExhibitionValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(128).WithMessage("Title must not exceed 128 characters")
            .When(x => x.Title != null);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value).WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.ExhibitorId)
            .GreaterThan(0).WithMessage("Exhibitor ID must be a positive number")
            .When(x => x.ExhibitorId.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(512).WithMessage("Description must not exceed 512 characters")
            .When(x => x.Description != null);
    }
}
