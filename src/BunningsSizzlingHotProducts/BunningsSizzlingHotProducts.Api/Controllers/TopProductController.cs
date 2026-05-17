using BunningsSizzlingHotProducts.Api.Contracts;
using BunningsSizzlingHotProducts.Application.Handlers;
using BunningsSizzlingHotProducts.Application.Queries;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BunningsSizzlingHotProducts.Api.Controllers;

[ApiController]
[Route("api/top-product")]
public sealed class TopProductController(
    GetDailyTopProductHandler dailyHandler,
    GetRollingTopProductHandler rollingHandler,
    IValidator<GetDailyTopProductQuery> dailyValidator,
    IValidator<GetRollingTopProductQuery> rollingValidator) : ControllerBase
{
    [HttpGet("daily")]
    [ProducesResponseType<TopProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TopProductResponse>> GetDaily(
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken)
    {
        var query = new GetDailyTopProductQuery(date);
        var validation = await dailyValidator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            return ValidationProblem(BuildModelState(validation.Errors));

        var result = await dailyHandler.HandleAsync(query, cancellationToken);
        return new TopProductResponse(result.From, result.To, result.ProductName);
    }

    [HttpGet("rolling")]
    [ProducesResponseType<TopProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TopProductResponse>> GetRolling(
        [FromQuery] int days = 3,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRollingTopProductQuery(days);
        var validation = await rollingValidator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            return ValidationProblem(BuildModelState(validation.Errors));

        var result = await rollingHandler.HandleAsync(query, cancellationToken);
        return new TopProductResponse(result.From, result.To, result.ProductName);
    }

    private static ModelStateDictionary BuildModelState(
        IEnumerable<FluentValidation.Results.ValidationFailure> errors)
    {
        var ms = new ModelStateDictionary();
        foreach (var e in errors) ms.AddModelError(e.PropertyName, e.ErrorMessage);
        return ms;
    }
}
