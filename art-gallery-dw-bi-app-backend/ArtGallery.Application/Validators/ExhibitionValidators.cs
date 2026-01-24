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
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.Budget)
            .GreaterThan(0).WithMessage("Budget must be greater than 0")
            .When(x => x.Budget.HasValue);

        RuleFor(x => x.ExpectedVisitors)
            .GreaterThan(0).WithMessage("Expected visitors must be greater than 0")
            .When(x => x.ExpectedVisitors.HasValue);

        RuleFor(x => x.Location)
            .MaximumLength(100).WithMessage("Location must not exceed 100 characters");

        RuleFor(x => x.Curator)
            .MaximumLength(100).WithMessage("Curator name must not exceed 100 characters");
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
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters")
            .When(x => x.Title != null);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.Budget)
            .GreaterThan(0).WithMessage("Budget must be greater than 0")
            .When(x => x.Budget.HasValue);
    }
}
