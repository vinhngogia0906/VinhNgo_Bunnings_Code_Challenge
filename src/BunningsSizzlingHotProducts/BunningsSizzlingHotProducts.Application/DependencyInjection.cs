using BunningsSizzlingHotProducts.Application.Handlers;
using BunningsSizzlingHotProducts.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BunningsSizzlingHotProducts.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<GetDailyTopProductHandler>();
        services.AddScoped<GetRollingTopProductHandler>();

        // Add every validator class that implements AbstractValidator
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));
        return services;
    }
}
