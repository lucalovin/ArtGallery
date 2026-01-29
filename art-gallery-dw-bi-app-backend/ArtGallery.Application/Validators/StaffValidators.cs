using FluentValidation;
using ArtGallery.Application.DTOs.Staff;

namespace ArtGallery.Application.Validators;

/// <summary>
/// Validator for CreateStaffDto.
/// </summary>
public class CreateStaffValidator : AbstractValidator<CreateStaffDto>
{
    public CreateStaffValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(128).WithMessage("Name must not exceed 128 characters");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .MaximumLength(64).WithMessage("Role must not exceed 64 characters");

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Hire date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Hire date cannot be in the future");

        RuleFor(x => x.CertificationLevel)
            .MaximumLength(32).WithMessage("Certification level must not exceed 32 characters")
            .When(x => x.CertificationLevel != null);
    }
}

/// <summary>
/// Validator for UpdateStaffDto.
/// </summary>
public class UpdateStaffValidator : AbstractValidator<UpdateStaffDto>
{
    public UpdateStaffValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(128).WithMessage("Name must not exceed 128 characters")
            .When(x => x.Name != null);

        RuleFor(x => x.Role)
            .MaximumLength(64).WithMessage("Role must not exceed 64 characters")
            .When(x => x.Role != null);

        RuleFor(x => x.HireDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Hire date cannot be in the future")
            .When(x => x.HireDate.HasValue);

        RuleFor(x => x.CertificationLevel)
            .MaximumLength(32).WithMessage("Certification level must not exceed 32 characters")
            .When(x => x.CertificationLevel != null);
    }
}
