using BunningsSizzlingHotProducts.Application.Queries;
using FluentValidation;

namespace BunningsSizzlingHotProducts.Application.Validators;

public sealed class GetRollingTopProductValidator : AbstractValidator<GetRollingTopProductQuery>
{
    public GetRollingTopProductValidator()
    {
        RuleFor(q => q.Days)
            .GreaterThan(0).WithMessage("Days must be positive.")
            .LessThanOrEqualTo(365).WithMessage("Days cannot exceed 365.");
    }
}
