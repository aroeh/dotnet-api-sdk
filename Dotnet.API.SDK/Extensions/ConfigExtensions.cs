using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Dotnet.API.SDK.Extensions;

public static class ConfigExtensions
{
    /// <summary>
    /// Adds configuration for the application from settings and environment variables
    /// </summary>
    /// <remarks>
    /// Sets the base path for the current directory and adds sources from lowest to higest priority
    /// appsettings.json, appsettings.{environment}.json, environment variables
    /// </remarks>
    /// <param name="configurationManager">Application configuration manager instance</param>
    public static void AddConfigSettings(this ConfigurationManager configurationManager)
    {
        // Read appsettings files
        configurationManager
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json")
            .AddEnvironmentVariables();
    }

    /// <summary>
    /// Adds configuration for the application from settings, environment variables, and user secrets
    /// </summary>
    /// <remarks>
    /// Sets the base path for the current directory and adds sources from lowest to higest priority
    /// appsettings.json, appsettings.{environment}.json, environment variables, user secrets
    /// </remarks>
    /// <typeparam name="TClass">Class type within the project or assembly that contains user secrets</typeparam>
    /// <param name="configurationManager">Application configuration manager instance</param>
    public static void AddConfigSettings<TClass>(this ConfigurationManager configurationManager) where TClass : class
    {
        // Read appsettings files
        configurationManager
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json")
            .AddEnvironmentVariables()
            .AddUserSecrets<TClass>();
    }

    /// <summary>
    /// Adds configuration for the application from settings, environment variables, and user secrets
    /// </summary>
    /// <remarks>
    /// Sets the base path for the current directory and adds sources from lowest to higest priority
    /// appsettings.json, appsettings.{environment}.json, environment variables, user secrets
    /// </remarks>
    /// <param name="configurationManager">Application configuration manager instance</param>
    /// <param name="userSecretsAssembly">Assembly that contains the user secrets id</param>
    public static void AddConfigSettings(this ConfigurationManager configurationManager, Assembly userSecretsAssembly)
    {
        // Read appsettings files
        configurationManager
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json")
            .AddEnvironmentVariables()
            .AddUserSecrets(userSecretsAssembly);
    }
}
