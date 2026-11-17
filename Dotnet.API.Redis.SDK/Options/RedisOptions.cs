namespace Dotnet.API.Redis.SDK.Options;

/// <summary>
/// Options configuration for Redis Cache
/// </summary>
public record RedisOptions
{
    /// <summary>
    /// Default Key for configuration values
    /// </summary>
    public const string ConfigKey = "Api:Settings:Redis";

    /// <summary>
    /// Time in minutes for cache value expiration
    /// </summary>
    public double Expiration { get; set; }

    /// <summary>
    /// Time in seconds for cache operation timeout
    /// </summary>
    public double Timeout { get; set; }

    /// <summary>
    /// Host connection for the Redis database
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Port number for connecting to the Redis database
    /// </summary>
    public int? Port { get; set; }
}
