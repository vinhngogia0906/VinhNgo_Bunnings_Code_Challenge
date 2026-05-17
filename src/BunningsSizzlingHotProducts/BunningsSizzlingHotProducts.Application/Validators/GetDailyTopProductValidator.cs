using BunningsSizzlingHotProducts.Application.Abstractions;
using BunningsSizzlingHotProducts.Application.Queries;
using FluentValidation;

namespace BunningsSizzlingHotProducts.Application.Validators;

public sealed class GetDailyTopProductValidator : AbstractValidator<GetDailyTopProductQuery>
{
    public GetDailyTopProductValidator(IClock clock)
    {
        RuleFor(q => q.Date)
            .NotEqual(default(DateOnly))
            .WithMessage("Date is required.")
            .Must(d => d <= clock.Today)
            .WithMessage("Date cannot be in the future.");
    }
}
