using FluentValidation;
using ArtGallery.Application.DTOs.Review;

namespace ArtGallery.Application.Validators;

/// <summary>
/// Validator for CreateReviewDto.
/// </summary>
public class CreateReviewValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewValidator()
    {
        // Must have either VisitorId or NewVisitor
        RuleFor(x => x)
            .Must(x => x.VisitorId.HasValue || x.NewVisitor != null)
            .WithMessage("Either an existing visitor ID or new visitor information must be provided");

        // If NewVisitor is provided, validate it
        When(x => x.NewVisitor != null, () =>
        {
            RuleFor(x => x.NewVisitor!.Name)
                .NotEmpty().WithMessage("Visitor name is required")
                .MaximumLength(128).WithMessage("Visitor name must not exceed 128 characters");

            RuleFor(x => x.NewVisitor!.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(128).WithMessage("Email must not exceed 128 characters")
                .When(x => !string.IsNullOrEmpty(x.NewVisitor!.Email));

            RuleFor(x => x.NewVisitor!.Phone)
                .MaximumLength(32).WithMessage("Phone must not exceed 32 characters")
                .When(x => x.NewVisitor!.Phone != null);

            RuleFor(x => x.NewVisitor!.MembershipType)
                .MaximumLength(32).WithMessage("Membership type must not exceed 32 characters")
                .When(x => x.NewVisitor!.MembershipType != null);
        });

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.ReviewText)
            .MaximumLength(256).WithMessage("Review text must not exceed 256 characters")
            .When(x => x.ReviewText != null);

        RuleFor(x => x.ReviewDate)
            .NotEmpty().WithMessage("Review date is required")
            .LessThanOrEqualTo(DateTime.Today.AddDays(1)).WithMessage("Review date cannot be in the future");

        // Must review at least an artwork or exhibition
        RuleFor(x => x)
            .Must(x => x.ArtworkId.HasValue || x.ExhibitionId.HasValue)
            .WithMessage("A review must be for at least an artwork or an exhibition");
    }
}

/// <summary>
/// Validator for UpdateReviewDto.
/// </summary>
public class UpdateReviewValidator : AbstractValidator<UpdateReviewDto>
{
    public UpdateReviewValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5")
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.ReviewText)
            .MaximumLength(256).WithMessage("Review text must not exceed 256 characters")
            .When(x => x.ReviewText != null);

        RuleFor(x => x.ReviewDate)
            .LessThanOrEqualTo(DateTime.Today.AddDays(1)).WithMessage("Review date cannot be in the future")
            .When(x => x.ReviewDate.HasValue);
    }
}
