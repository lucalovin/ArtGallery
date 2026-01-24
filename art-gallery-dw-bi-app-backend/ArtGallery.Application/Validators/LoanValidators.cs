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
            .GreaterThan(0).WithMessage("Artwork ID is required");

        RuleFor(x => x.BorrowerName)
            .NotEmpty().WithMessage("Borrower name is required")
            .MaximumLength(255).WithMessage("Borrower name must not exceed 255 characters");

        RuleFor(x => x.LoanStartDate)
            .NotEmpty().WithMessage("Loan start date is required");

        RuleFor(x => x.LoanEndDate)
            .NotEmpty().WithMessage("Loan end date is required")
            .GreaterThan(x => x.LoanStartDate).WithMessage("Loan end date must be after start date");

        RuleFor(x => x.InsuranceValue)
            .GreaterThan(0).WithMessage("Insurance value must be greater than 0")
            .When(x => x.InsuranceValue.HasValue);

        RuleFor(x => x.LoanFee)
            .GreaterThanOrEqualTo(0).WithMessage("Loan fee cannot be negative")
            .When(x => x.LoanFee.HasValue);
    }
}

/// <summary>
/// Validator for UpdateLoanDto.
/// </summary>
public class UpdateLoanValidator : AbstractValidator<UpdateLoanDto>
{
    public UpdateLoanValidator()
    {
        RuleFor(x => x.LoanEndDate)
            .GreaterThan(x => x.LoanStartDate).WithMessage("Loan end date must be after start date")
            .When(x => x.LoanStartDate.HasValue && x.LoanEndDate.HasValue);

        RuleFor(x => x.InsuranceValue)
            .GreaterThan(0).WithMessage("Insurance value must be greater than 0")
            .When(x => x.InsuranceValue.HasValue);
    }
}
