namespace Dotnet.API.SDK.Options;

/// <summary>
/// Options configuration for Health Checks
/// </summary>
public record ApiHealthCheckOptions
{
    /// <summary>
    /// Default Key for configuration values
    /// </summary>
    public const string ConfigKey = "Api:Settings:HealthCheck";

    /// <summary>
    /// Array of configured routes for API Health Checks
    /// </summary>
    public ApiHealthRoute[]? Routes { get; set; }
}

public record ApiHealthRoute
{
    /// <summary>
    /// Route to use when calling the health check endpoint
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// Tag to filter health check results on the route
    /// </summary>
    public string[]? Tags { get; set; }
}