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
    /// Route to use when calling the health check endpoint
    /// </summary>
    public string? Route { get; set; }
}
