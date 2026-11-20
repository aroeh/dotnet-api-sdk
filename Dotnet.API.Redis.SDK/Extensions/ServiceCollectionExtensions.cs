using Dotnet.API.Redis.SDK.Interfaces;
using Dotnet.API.Redis.SDK.Options;
using Dotnet.API.Redis.SDK.Repos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Dotnet.API.Redis.SDK.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configure and add Redis options to the application service collection.
    /// </summary>
    /// <param name="services">Application services collection</param>
    /// <param name="config">Application configuration properties</param>
    /// <param name="configKey">Custom configuration key to retrieve options from app settings.  If <see langword="null"/> or empty, the value defaults to Api:Settings:Redis</param>
    /// <exception cref="Exception">Exceptions are thrown if the ConnectionString or DatabaseName properties of options are null, empty, or whitespace values.</exception>
    public static void ConfigureRedisOptions(this IServiceCollection services, IConfiguration config, string? configKey = null)
    {
        Console.Write("Adding Redis options...");

        var configSettings = GetRedisConfigSettings(config, configKey);

        services.Configure<RedisOptions>(configSettings);

        Console.WriteLine("Complete");
    }

    /// <summary>
    /// Adds a singleton instance of a <see cref="IConnectionMultiplexer"/> to the application service collection.
    /// </summary>
    /// <param name="services">Application services collection</param>
    /// <param name="config">Application configuration properties</param>
    /// <param name="configKey">Custom configuration key to retrieve options from app settings.  If <see langword="null"/> or empty, the value defaults to Api:Settings:Redis</param>
    public static void AddRedisConnectionMultiplexer(this IServiceCollection services, IConfiguration config, string? configKey = null)
    {
        Console.Write("Adding Redis Connection Multiplexor...");

        var options = GetRedisOptions(config, configKey);

        var hostAndPort = $"{options.Host}:{options.Port}";
        var redisConfig = new ConfigurationOptions();
        redisConfig.EndPoints.Add(hostAndPort);
        var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfig);

        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

        Console.WriteLine("Complete");
    }

    /// <summary>
    /// Adds a full set of redis services and scoped repos to the application service collection.
    /// </summary>
    /// <remarks>
    /// Adds the following instances to the application service collection:
    /// <para>1. <see cref="RedisOptions"/></para>
    /// <para>2. Adds singleton instance of a <see cref="IConnectionMultiplexer"/></para>
    /// <para>3. A scoped instance of <see cref="IRedisRepo"/>.</para>
    /// </remarks>
    /// <param name="services">Application services collection</param>
    /// <param name="config">Application configuration properties</param>
    /// <param name="configKey">Custom configuration key to retrieve options from app settings.  If <see langword="null"/> or empty, the value defaults to Api:Settings:Redis</param>
    public static void AddRedisScopedServices(this IServiceCollection services, IConfiguration config, string? configKey = null)
    {
        services.ConfigureRedisOptions(config, configKey);
        services.AddRedisConnectionMultiplexer(config, configKey);

        Console.Write("Adding Scoped RedisRepo...");
        services.AddScoped<IRedisRepo, RedisRepo>();
        Console.WriteLine("Complete");
    }

    /// <summary>
    /// Adds a full set of redis services and transient repos to the application service collection.
    /// </summary>
    /// <remarks>
    /// Adds the following instances to the application service collection:
    /// <para>1. <see cref="RedisOptions"/></para>
    /// <para>2. Adds singleton instance of a <see cref="IConnectionMultiplexer"/></para>
    /// <para>3. A transient instance of <see cref="IRedisRepo"/>.</para>
    /// </remarks>
    /// <param name="services">Application services collection</param>
    /// <param name="config">Application configuration properties</param>
    /// <param name="configKey">Custom configuration key to retrieve options from app settings.  If <see langword="null"/> or empty, the value defaults to Api:Settings:Redis</param>
    public static void AddRedisTransientServices(this IServiceCollection services, IConfiguration config, string? configKey = null)
    {
        services.ConfigureRedisOptions(config, configKey);
        services.AddRedisConnectionMultiplexer(config, configKey);

        Console.Write("Adding Transient RedisRepo...");
        services.AddTransient<IRedisRepo, RedisRepo>();
        Console.WriteLine("Complete");
    }

    private static IConfigurationSection GetRedisConfigSettings(IConfiguration config, string? configKey = null)
    {
        var configSettings = string.IsNullOrWhiteSpace(configKey)
            ? config.GetRequiredSection(RedisOptions.ConfigKey)
            : config.GetRequiredSection(configKey);

        return configSettings;
    }

    private static RedisOptions GetRedisOptions(IConfiguration config, string? configKey = null)
    {
        var configSettings = GetRedisConfigSettings(config, configKey);

        var options = configSettings.Get<RedisOptions>() ?? throw new Exception("Options for Redis were not found.");

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            throw new Exception("Redis Host string is missing");
        }

        if (options.Port is null)
        {
            throw new Exception("Redis Port is missing");
        }

        if (options.Port is not null && options.Port <= 0)
        {
            throw new Exception("Redis Port cannot be a negative number.");
        }

        if (options.Expiration <= 0)
        {
            throw new Exception("Redis Expiration minutes must be a positive number.");
        }

        if (options.Timeout <= 0)
        {
            throw new Exception("Redis Timeout seconds must be a positive number.");
        }

        return options;
    }
}
