using Demo.API.Core.Interfaces;
using Dotnet.API.Core.Example.Orchestrations;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.API.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreOrchestrations(this IServiceCollection services)
    {
        services.AddScoped<IRestuarantOrchestration, RestuarantOrchestration>();
        services.AddScoped<IBusinessHoursOrchestration, BusinessHoursOrchestration>();

        return services;
    }
}