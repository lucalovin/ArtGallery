using FluentValidation;
using ArtGallery.Application.DTOs.Artwork;

namespace ArtGallery.Application.Validators;

/// <summary>
/// Validator for CreateArtworkDto.
/// </summary>
public class CreateArtworkValidator : AbstractValidator<CreateArtworkDto>
{
    public CreateArtworkValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters");

        RuleFor(x => x.Artist)
            .NotEmpty().WithMessage("Artist is required")
            .MaximumLength(255).WithMessage("Artist must not exceed 255 characters");

        RuleFor(x => x.Year)
            .GreaterThan(0).WithMessage("Year must be a positive number")
            .LessThanOrEqualTo(DateTime.Now.Year).WithMessage("Year cannot be in the future");

        RuleFor(x => x.Medium)
            .NotEmpty().WithMessage("Medium is required")
            .MaximumLength(100).WithMessage("Medium must not exceed 100 characters");

        RuleFor(x => x.Dimensions)
            .NotEmpty().WithMessage("Dimensions are required")
            .MaximumLength(100).WithMessage("Dimensions must not exceed 100 characters");

        RuleFor(x => x.Collection)
            .NotEmpty().WithMessage("Collection is required")
            .MaximumLength(100).WithMessage("Collection must not exceed 100 characters");

        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("Status must not exceed 50 characters");

        RuleFor(x => x.EstimatedValue)
            .GreaterThan(0).WithMessage("Estimated value must be greater than 0")
            .When(x => x.EstimatedValue.HasValue);

        RuleFor(x => x.AcquisitionDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Acquisition date cannot be in the future");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters");

        RuleFor(x => x.Location)
            .MaximumLength(100).WithMessage("Location must not exceed 100 characters");
    }
}

/// <summary>
/// Validator for UpdateArtworkDto.
/// </summary>
public class UpdateArtworkValidator : AbstractValidator<UpdateArtworkDto>
{
    public UpdateArtworkValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters")
            .When(x => x.Title != null);

        RuleFor(x => x.Artist)
            .MaximumLength(255).WithMessage("Artist must not exceed 255 characters")
            .When(x => x.Artist != null);

        RuleFor(x => x.Year)
            .GreaterThan(0).WithMessage("Year must be a positive number")
            .LessThanOrEqualTo(DateTime.Now.Year).WithMessage("Year cannot be in the future")
            .When(x => x.Year.HasValue);

        RuleFor(x => x.EstimatedValue)
            .GreaterThan(0).WithMessage("Estimated value must be greater than 0")
            .When(x => x.EstimatedValue.HasValue);
    }
}
