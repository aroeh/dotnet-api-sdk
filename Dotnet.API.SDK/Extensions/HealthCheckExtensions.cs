using Dotnet.API.SDK.Middleware;
using Dotnet.API.SDK.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Dotnet.API.SDK.Extensions;

public static class HealthCheckExtensions
{
    /// <summary>
    /// Add health check registrations to the application service instance.
    /// </summary>
    /// <typeparam name="T">Health Check class instance that implements <see cref="IHealthCheck"/></typeparam>
    /// <param name="services">Application services collection</param>
    /// <param name="name">Name of the health check registration.  If not provided the name will be derived from the name of type <typeparamref name="T"/></param>
    /// <param name="failureStatus"><see cref="HealthStatus"/> to use for a failure state.  If not provided a default value of HealthStatus.Unhealthy will be used.</param>
    /// <param name="tags">Tags to be provided for health check filters.  Tags must be mapped to an endpoint in order to be used as a filter.  <seealso cref="UseHealthChecks"/></param>
    /// <param name="timeout">Timeout of the health check.</param>
    public static void AddHealth<T>(this IServiceCollection services, string? name = null, HealthStatus? failureStatus = null, string[]? tags = null, TimeSpan? timeout = null) where T : class, IHealthCheck
    {
        string registrationName = string.IsNullOrWhiteSpace(name) ? typeof(T).Name : name;
        
        Console.Write($"Adding application health check for {registrationName}...");

        services.AddHealthChecks()
            .AddCheck<T>(registrationName, failureStatus, tags, timeout);

        Console.WriteLine("Complete");
    }

    /// <summary>
    /// Maps healthcheck endpoints point for use in the application.
    /// </summary>
    /// <remarks>
    /// Set the health check endpoints by retrieving the route from configuration values
    /// </remarks>
    /// <param name="app">Application builder instance</param>
    /// <param name="config">Application configuration properties</param>
    /// <param name="configKey">Custom configuration key to retrieve options from app settings.  If <see langword="null"/> or empty, the value defaults to Api:Settings:HealthEndpoint</param>
    /// <exception cref="Exception">Exceptions are thrown if the Route property of options is a null, empty, or whitespace value.</exception>
    public static void UseHealthChecks(this WebApplication app, IConfiguration config, string? configKey = null)
    {
        app.Logger.LogInformation("Preparing app health checks for use.");

        var configSettings = string.IsNullOrWhiteSpace(configKey)
            ? config.GetRequiredSection(ApiHealthCheckOptions.ConfigKey)
            : config.GetRequiredSection(configKey);

        var options = configSettings.Get<ApiHealthCheckOptions>() ?? throw new Exception("Options for Healthcheck were not found.");

        if (options.Routes is null || options.Routes.Length == 0)
        {
            throw new Exception("HealthRoutes were not found in the configuration for health checks.");
        }

        foreach (var routeConfig in options.Routes)
        {
            UseHealthChecks(app, routeConfig.Route, routeConfig.Tags);
        }
    }

    /// <summary>
    /// Maps a single healthcheck endpoint point for use in the application.
    /// </summary>
    /// <remarks>
    /// Set the health check endpoint directly with a named route and tags.  
    /// Tags are used to filter health check requests when calling the health check routes.
    /// </remarks>
    /// <param name="app">Application builder instance</param>
    /// <param name="route">The route for the healthcheck endpoint.</param>
    /// <param name="tags">Optional collection of tags for filtering health check results on the mapped route.</param>
    /// <exception cref="Exception">Exceptions are thrown if the provided route is a null, empty, or whitespace value.</exception>
    public static void UseHealthChecks(this WebApplication app, string? route, string[]? tags = null)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            throw new Exception("Healthcheck route is missing.");
        }

        HealthCheckOptions healthCheckOptions = new()
        {
            ResponseWriter = HealthCheckResponseWriter.WriteCustomHealthCheckResponse
        };

        if (tags is not null && tags.Length > 0)
        {
            healthCheckOptions.Predicate = healthCheck =>
                healthCheck.Tags.Any(t => tags.Contains(t, StringComparer.OrdinalIgnoreCase));
        }

        app.MapHealthChecks(route, healthCheckOptions);

        app.Logger.LogInformation("App health checks ready for use at route: {route}", route);
    }
}
