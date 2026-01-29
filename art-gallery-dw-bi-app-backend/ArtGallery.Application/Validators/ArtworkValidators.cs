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
            .MaximumLength(128).WithMessage("Title must not exceed 128 characters");

        RuleFor(x => x.ArtistId)
            .GreaterThan(0).WithMessage("Artist is required");

        RuleFor(x => x.YearCreated)
            .GreaterThan(0).WithMessage("Year must be a positive number")
            .LessThanOrEqualTo(DateTime.Now.Year).WithMessage("Year cannot be in the future")
            .When(x => x.YearCreated.HasValue);

        RuleFor(x => x.Medium)
            .MaximumLength(64).WithMessage("Medium must not exceed 64 characters")
            .When(x => x.Medium != null);

        RuleFor(x => x.EstimatedValue)
            .GreaterThan(0).WithMessage("Estimated value must be greater than 0")
            .When(x => x.EstimatedValue.HasValue);
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
            .MaximumLength(128).WithMessage("Title must not exceed 128 characters")
            .When(x => x.Title != null);

        RuleFor(x => x.ArtistId)
            .GreaterThan(0).WithMessage("Artist ID must be a positive number")
            .When(x => x.ArtistId.HasValue);

        RuleFor(x => x.YearCreated)
            .GreaterThan(0).WithMessage("Year must be a positive number")
            .LessThanOrEqualTo(DateTime.Now.Year).WithMessage("Year cannot be in the future")
            .When(x => x.YearCreated.HasValue);

        RuleFor(x => x.EstimatedValue)
            .GreaterThan(0).WithMessage("Estimated value must be greater than 0")
            .When(x => x.EstimatedValue.HasValue);
    }
}
