using BunningsSizzlingHotProducts.Application.Queries;
using BunningsSizzlingHotProducts.Application.Tests.Support;
using BunningsSizzlingHotProducts.Application.Validators;
using FluentValidation.TestHelper;

namespace BunningsSizzlingHotProducts.Application.Tests.Validators;

public class GetDailyTopProductValidatorTests
{
    private readonly GetDailyTopProductValidator _v =
        new(new FixedClock(new DateOnly(2026, 4, 23)));

    [Fact]
    public void Future_dates_are_rejected()
    {
        var result = _v.TestValidate(new GetDailyTopProductQuery(new DateOnly(2026, 4, 24)));
        result.ShouldHaveValidationErrorFor(q => q.Date);
    }

    [Fact]
    public void Today_is_accepted()
    {
        var result = _v.TestValidate(new GetDailyTopProductQuery(new DateOnly(2026, 4, 23)));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
