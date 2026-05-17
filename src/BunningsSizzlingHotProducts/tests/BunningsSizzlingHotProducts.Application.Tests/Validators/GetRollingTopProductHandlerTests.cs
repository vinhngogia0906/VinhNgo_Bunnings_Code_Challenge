using BunningsSizzlingHotProducts.Application.Queries;
using BunningsSizzlingHotProducts.Application.Tests.Support;
using BunningsSizzlingHotProducts.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace BunningsSizzlingHotProducts.Application.Tests.Validators;

public class GetRollingTopProductHandlerTests
{
    private readonly GetRollingTopProductValidator _v =
        new();

    [Fact]
    public void Negative_dates_number_is_rejected()
    {
        int numberOfDays = -2;
        var result = _v.TestValidate(new GetRollingTopProductQuery(numberOfDays));
        result.ShouldHaveValidationErrorFor(q => q.Days);
    }

    [Fact]
    public void Positive_dates_number_less_than_365_is_accepted()
    {
        int numberOfDays = 10;
        var result = _v.TestValidate(new GetRollingTopProductQuery(numberOfDays));
        result.ShouldNotHaveValidationErrorFor(q => q.Days);
    }

    [Fact]
    public void Positive_dates_number_greater_than_365_is_rejected()
    {
        int numberOfDays = 377;
        var result = _v.TestValidate(new GetRollingTopProductQuery(numberOfDays));
        result.ShouldHaveValidationErrorFor(q => q.Days);
    }

}
