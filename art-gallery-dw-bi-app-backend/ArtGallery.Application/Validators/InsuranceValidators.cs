using FluentValidation;
using ArtGallery.Application.DTOs.Insurance;

namespace ArtGallery.Application.Validators;

/// <summary>
/// Validator for CreateInsuranceDto.
/// </summary>
public class CreateInsuranceValidator : AbstractValidator<CreateInsuranceDto>
{
    public CreateInsuranceValidator()
    {
        RuleFor(x => x.ArtworkId)
            .GreaterThan(0).WithMessage("Artwork is required");

        RuleFor(x => x.PolicyId)
            .GreaterThan(0).WithMessage("Policy is required");

        RuleFor(x => x.InsuredAmount)
            .GreaterThan(0).WithMessage("Insured amount must be greater than 0");
    }
}

/// <summary>
/// Validator for UpdateInsuranceDto.
/// </summary>
public class UpdateInsuranceValidator : AbstractValidator<UpdateInsuranceDto>
{
    public UpdateInsuranceValidator()
    {
        RuleFor(x => x.ArtworkId)
            .GreaterThan(0).WithMessage("Artwork ID must be a positive number")
            .When(x => x.ArtworkId.HasValue);

        RuleFor(x => x.PolicyId)
            .GreaterThan(0).WithMessage("Policy ID must be a positive number")
            .When(x => x.PolicyId.HasValue);

        RuleFor(x => x.InsuredAmount)
            .GreaterThan(0).WithMessage("Insured amount must be greater than 0")
            .When(x => x.InsuredAmount.HasValue);
    }
}
