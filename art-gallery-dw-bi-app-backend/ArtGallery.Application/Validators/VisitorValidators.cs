using FluentValidation;
using ArtGallery.Application.DTOs.Visitor;

namespace ArtGallery.Application.Validators;

/// <summary>
/// Validator for CreateVisitorDto.
/// </summary>
public class CreateVisitorValidator : AbstractValidator<CreateVisitorDto>
{
    public CreateVisitorValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters");

        RuleFor(x => x.MembershipExpiry)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Membership expiry must be today or in the future")
            .When(x => x.MembershipExpiry.HasValue && x.MembershipType != "None");
    }
}

/// <summary>
/// Validator for UpdateVisitorDto.
/// </summary>
public class UpdateVisitorValidator : AbstractValidator<UpdateVisitorDto>
{
    public UpdateVisitorValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters")
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters")
            .When(x => x.LastName != null);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters")
            .When(x => x.Email != null);
    }
}
