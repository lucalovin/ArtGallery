using FluentValidation;
using ArtGallery.Application.DTOs.Loan;

namespace ArtGallery.Application.Validators;

/// <summary>
/// Validator for CreateLoanDto.
/// </summary>
public class CreateLoanValidator : AbstractValidator<CreateLoanDto>
{
    public CreateLoanValidator()
    {
        RuleFor(x => x.ArtworkId)
            .GreaterThan(0).WithMessage("Artwork is required");

        RuleFor(x => x.ExhibitorId)
            .GreaterThan(0).WithMessage("Exhibitor is required");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.Conditions)
            .MaximumLength(512).WithMessage("Conditions must not exceed 512 characters")
            .When(x => x.Conditions != null);
    }
}

/// <summary>
/// Validator for UpdateLoanDto.
/// </summary>
public class UpdateLoanValidator : AbstractValidator<UpdateLoanDto>
{
    public UpdateLoanValidator()
    {
        RuleFor(x => x.ArtworkId)
            .GreaterThan(0).WithMessage("Artwork ID must be a positive number")
            .When(x => x.ArtworkId.HasValue);

        RuleFor(x => x.ExhibitorId)
            .GreaterThan(0).WithMessage("Exhibitor ID must be a positive number")
            .When(x => x.ExhibitorId.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value).WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.Conditions)
            .MaximumLength(512).WithMessage("Conditions must not exceed 512 characters")
            .When(x => x.Conditions != null);
    }
}
