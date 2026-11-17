using Dotnet.API.SDK.Middleware;
using Dotnet.API.SDK.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dotnet.API.SDK.Extensions;

public static class HealthCheckExtensions
{
    /// <summary>
    /// Register health checks for the specified class types
    /// </summary>
    /// <param name="services">Application services collection</param>
    /// <param name="healthCheckTypes">Types of classes that implement the IHealthCheck interface</param>
    /// <exception cref="ArgumentException">Type exception if a parameter type is not a class, or is an abstract class, or does not implement IHealthCheck</exception>
    public static void AddHealth(this IServiceCollection services, params Type[] healthCheckTypes)
    {
        var healthCheckBuilder = services.AddHealthChecks();

        foreach (var healthCheckType in healthCheckTypes)
        {
            if (!healthCheckType.IsClass || healthCheckType.IsAbstract || !typeof(IHealthCheck).IsAssignableFrom(healthCheckType))
            {
                throw new ArgumentException(
                    $"{healthCheckType} must be a concrete {nameof(IHealthCheck)}.",
                    nameof(healthCheckTypes));
            }

            healthCheckBuilder.Add(new HealthCheckRegistration(
                healthCheckType.Name,
                serviceProvider => (IHealthCheck)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, healthCheckType),
                failureStatus: null,
                tags: null));
        }
    }

    /// <summary>
    /// Maps the healthcheck endpoint point for use in the application
    /// </summary>
    /// <remarks>
    /// Set the health check endpoint by retrieving the route from configuration values
    /// </remarks>
    /// <param name="app">Application builder instance</param>
    /// <param name="config">Application configuration properties</param>
    /// <param name="configKey">Custom configuration key to retrieve options from app settings.  If <see langword="null"/> or empty, the value defaults to Api:Settings:HealthEndpoint</param>
    /// <exception cref="Exception">Exceptions are thrown if the Route property of options is a null, empty, or whitespace value.</exception>
    public static void UseHealthChecks(this WebApplication app, IConfiguration config, string? configKey = null)
    {
        var configSettings = string.IsNullOrWhiteSpace(configKey)
            ? config.GetRequiredSection(ApiHealthCheckOptions.ConfigKey)
            : config.GetRequiredSection(configKey);

        var options = configSettings.Get<ApiHealthCheckOptions>() ?? throw new Exception("Options for Healthcheck were not found.");

        UseHealthChecks(app, options.Route);
    }

    /// <summary>
    /// Maps the healthcheck endpoint point for use in the application
    /// </summary>
    /// <remarks>
    /// Set the health check endpoint directly with a named route
    /// </remarks>
    /// <param name="app">Application builder instance</param>
    /// <param name="route">The route for the healthcheck endpoint.</param>
    /// <exception cref="Exception">Exceptions are thrown if the provided route is a null, empty, or whitespace value.</exception>
    public static void UseHealthChecks(this WebApplication app, string? route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            throw new Exception("Healthcheck route is missing.");
        }

        app.MapHealthChecks(route, new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteCustomHealthCheckResponse
        });
    }
}
