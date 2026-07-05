using Dotnet.API.MSSQL.SDK.Entities;
using Dotnet.API.MSSQL.SDK.Interfaces;
using Dotnet.API.MSSQL.SDK.Options;
using Dotnet.API.MSSQL.SDK.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.API.MSSQL.SDK.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configure and add MS SQL options to the application service collection instance.
    /// </summary>
    /// <param name="services">Application services collection</param>
    /// <param name="config">Application configuration properties</param>
    /// <param name="configKey">Custom configuration key to retrieve options from app settings.  If <see langword="null"/> or empty, the value defaults to Api:Settings:MSSql</param>
    /// <exception cref="Exception">Exceptions are thrown if the configuration settings are not found.</exception>
    public static void ConfigureMSSqlOptions(this IServiceCollection services, IConfiguration config, string? configKey = null)
    {
        var configSettings = GetMSSqlConfigurationSection(config, configKey);

        services.Configure<MSSqlOptions>(configSettings);
    }

    /// <summary>
    /// Add Entity Framework Database context to the application service collection instance.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="services">Application services collection</param>
    /// <param name="config">Application configuration properties</param>
    /// <param name="configKey">Custom configuration key to retrieve options from app settings.  If <see langword="null"/> or empty, the value defaults to Api:Settings:MSSql</param>
    /// <exception cref="Exception">Exceptions are thrown if the ConnectionString property of options are null, empty, or whitespace values.</exception>
    /// <returns>Application services collection</returns>
    public static IServiceCollection AddEFCoreDbContext<TContext>(this IServiceCollection services, IConfiguration config, string? configKey = null) where TContext : DbContext
    {
        var sqlOptions = GetOptions(config, configKey);

        services.AddDbContext<TContext>(
            options => options.UseSqlServer(sqlOptions.ConnectionString)
        );

        return services;
    }

    /// <summary>
    /// Adds a Scoped EFSqlRepo repo instance to the service collection
    /// </summary>
    /// <typeparam name="TContext"><see cref="DbContext"/> class instance to register with the EFSqlRepo repo.</typeparam>
    /// <typeparam name="TEntity">Data entity to register with the EFSqlRepo repo.</typeparam>
    /// <param name="services">Application services collection</param>
    /// <returns>Application services collection</returns>
    public static IServiceCollection AddScopedSqlRepo<TContext, TEntity>(this IServiceCollection services)
        where TContext : DbContext
        where TEntity : EntityBase
    {
        services.AddScoped<IEFSqlRepo<TContext, TEntity>, EFSqlRepo<TContext, TEntity>>();
        return services;
    }

    private static IConfigurationSection GetMSSqlConfigurationSection(IConfiguration config, string? configKey = null)
    {
        return string.IsNullOrWhiteSpace(configKey)
            ? config.GetRequiredSection(MSSqlOptions.ConfigKey)
            : config.GetRequiredSection(configKey);
    }

    private static MSSqlOptions GetOptions(IConfiguration config, string? configKey = null)
    {
        var configSettings = GetMSSqlConfigurationSection(config, configKey);

        var options = configSettings.Get<MSSqlOptions>() ?? throw new Exception("Options for SQL were not found.");

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new Exception("SQL Connection string is missing");
        }

        return options;
    }
}
