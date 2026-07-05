using Dotnet.API.MongoDb.SDK.Interfaces;
using Dotnet.API.MongoDb.SDK.Options;
using Dotnet.API.MongoDb.SDK.Repos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dotnet.API.MongoDb.SDK.Extensions;

/// <summary>
/// MongoDb service collection extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configure and add MongoDb options in the application service collection instance
    /// </summary>
    /// <param name="services">Application services collection</param>
    /// <param name="config">Application configuration properties</param>
    /// <param name="configKey">Custom configuration key to retrieve options from app settings.  If <see langword="null"/> or empty, the value defaults to Api:Settings:MongoDb</param>
    /// <exception cref="Exception">Exceptions are thrown if the ConnectionString or DatabaseName properties of options are null, empty, or whitespace values.</exception>
    public static void ConfigureMongoDbOptions(this IServiceCollection services, IConfiguration config, string? configKey = null)
    {
        var configSettings = string.IsNullOrWhiteSpace(configKey)
            ? config.GetRequiredSection(MongoDbOptions.ConfigKey)
            : config.GetRequiredSection(configKey);

        var options = configSettings.Get<MongoDbOptions>();

        if (options is not null)
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new Exception("MongoDb Connection string is missing");
            }

            if (string.IsNullOrWhiteSpace(options.DatabaseName))
            {
                throw new Exception("MongoDb database name is missing");
            }
        }

        services.Configure<MongoDbOptions>(configSettings);
    }

    /// <summary>
    /// Adds a Scoped MongoDb repo instance to the service collection
    /// </summary>
    /// <typeparam name="TEntity">Data entity to register with the MongoDb repo</typeparam>
    /// <param name="services">Application services collection</param>
    /// <param name="collectionName">Name of the MongoDb collection the repo will use</param>
    public static void AddScopedMongoDbRepo<TEntity>(this IServiceCollection services, string collectionName) where TEntity : class
    {
        services.AddScoped<IMongoDbRepo<TEntity>, MongoDbRepo<TEntity>>(sp =>
        {
            ILogger<MongoDbRepo<TEntity>> logger = sp.GetRequiredService<ILogger<MongoDbRepo<TEntity>>>();
            IOptions<MongoDbOptions> options = sp.GetRequiredService<IOptions<MongoDbOptions>>();
            return new MongoDbRepo<TEntity>(logger, options, collectionName);
        });
    }

    /// <summary>
    /// Adds a Transient MongoDb repo instance to the service collection
    /// </summary>
    /// <typeparam name="TEntity">Data entity to register with the MongoDb repo</typeparam>
    /// <param name="services">Application services collection</param>
    /// <param name="collectionName">Name of the MongoDb collection the repo will use</param>
    public static void AddTransientMongoDbRepo<TEntity>(this IServiceCollection services, string collectionName) where TEntity : class
    {
        services.AddTransient<IMongoDbRepo<TEntity>, MongoDbRepo<TEntity>>(sp =>
        {
            ILogger<MongoDbRepo<TEntity>> logger = sp.GetRequiredService<ILogger<MongoDbRepo<TEntity>>>();
            IOptions<MongoDbOptions> options = sp.GetRequiredService<IOptions<MongoDbOptions>>();
            return new MongoDbRepo<TEntity>(logger, options, collectionName);
        });
    }
}
