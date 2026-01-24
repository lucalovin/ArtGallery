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

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required")
            .MaximumLength(50).WithMessage("Department must not exceed 50 characters");

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Position is required")
            .MaximumLength(100).WithMessage("Position must not exceed 100 characters");

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Hire date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Hire date cannot be in the future");

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than 0")
            .When(x => x.Salary.HasValue);
    }
}

/// <summary>
/// Validator for UpdateStaffDto.
/// </summary>
public class UpdateStaffValidator : AbstractValidator<UpdateStaffDto>
{
    public UpdateStaffValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => x.Email != null);

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than 0")
            .When(x => x.Salary.HasValue);
    }
}
