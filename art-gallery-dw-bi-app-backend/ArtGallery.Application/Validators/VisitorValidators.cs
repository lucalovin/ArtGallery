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
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(128).WithMessage("Name must not exceed 128 characters");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(128).WithMessage("Email must not exceed 128 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(32).WithMessage("Phone must not exceed 32 characters")
            .When(x => x.Phone != null);

        RuleFor(x => x.MembershipType)
            .MaximumLength(32).WithMessage("Membership type must not exceed 32 characters")
            .When(x => x.MembershipType != null);
    }
}

/// <summary>
/// Validator for UpdateVisitorDto.
/// </summary>
public class UpdateVisitorValidator : AbstractValidator<UpdateVisitorDto>
{
    public UpdateVisitorValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(128).WithMessage("Name must not exceed 128 characters")
            .When(x => x.Name != null);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(128).WithMessage("Email must not exceed 128 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(32).WithMessage("Phone must not exceed 32 characters")
            .When(x => x.Phone != null);

        RuleFor(x => x.MembershipType)
            .MaximumLength(32).WithMessage("Membership type must not exceed 32 characters")
            .When(x => x.MembershipType != null);
    }
}
