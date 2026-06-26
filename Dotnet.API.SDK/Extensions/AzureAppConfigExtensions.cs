using Dotnet.API.SDK.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.API.SDK.Extensions;

public static class AzureAppConfigExtensions
{
    /// <summary>
    /// Adds Azure App Configuration Settings
    /// </summary>
    /// <param name="configurationManager">Application configuration manager instance</param>
    /// <param name="labelFilters">Label filters for configuration settings.  At least one label is required.</param>
    /// <param name="keyFilters">Key filters for configuration settings.  Optional, if <see langword="null"/> or empty, the filter defaults to Any key value</param>
    /// <remarks>
    /// The filters will match provided label filters to each key provided.  If no key is provided, then it will default to Any
    /// and each label will be selected using a key of Any.
    /// </remarks>
    public static void AddAzureAppConfigSettings(this ConfigurationManager configurationManager, HashSet<string> labelFilters, HashSet<string>? keyFilters = null)
    {
        AppConfigOptions appConfigOptions = GetOptions(configurationManager);

        Dictionary<string, HashSet<string>> filters = BuildFilters(labelFilters, keyFilters);

        configurationManager.AddAzureAppConfiguration(options =>
        {
            ConnectToAzureAppConfig(options, appConfigOptions);

            // get app config key values without labels
            options.Select(KeyFilter.Any, LabelFilter.Null);

            // get app config key values using key and label filters
            foreach (var filterKey in filters)
            {
                foreach (var labelFilter in filterKey.Value)
                {
                    options.Select(filterKey.Key, labelFilter);
                }
            }
        });
    }

    private static AppConfigOptions GetOptions(IConfiguration config)
    {
        var configSettings = config.GetRequiredSection(AppConfigOptions.ConfigKey);

        var options = configSettings.Get<AppConfigOptions>();

        if (options is not null)
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString) && string.IsNullOrWhiteSpace(options.Endpoint))
            {
                throw new Exception("The App Config `ConnectionString` and `Endpoint` are both missing.  One or other is required.");
            }

            if (!string.IsNullOrWhiteSpace(options.ConnectionString) && !string.IsNullOrWhiteSpace(options.Endpoint))
            {
                throw new Exception("The App Config `ConnectionString` and `Endpoint` are both set.  Only one must be configured.");
            }
        }

        return options!;
    }

    private static Dictionary<string, HashSet<string>> BuildFilters(HashSet<string> labelFilters, HashSet<string>? keyFilters = null)
    {
        if (labelFilters.Count == 0 || labelFilters.All(l => string.IsNullOrWhiteSpace(l)))
        {
            throw new Exception("At least one label is required to retrieve azure app config settings.");
        }

        if (keyFilters is null || keyFilters.Count == 0)
        {
            return new Dictionary<string, HashSet<string>>()
            {
                { KeyFilter.Any, labelFilters }
            };
        }

        Dictionary<string, HashSet<string>> keyLabelFilterDictionary = [];
        foreach (var key in keyFilters)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                keyLabelFilterDictionary.Add(key, labelFilters);
            }
        }

        return keyLabelFilterDictionary;
    }

    private static void ConnectToAzureAppConfig(AzureAppConfigurationOptions options, AppConfigOptions appConfigOptions)
    {
        if (!string.IsNullOrWhiteSpace(appConfigOptions.ConnectionString))
        {
            options.Connect(appConfigOptions.ConnectionString);
        }

        if (!string.IsNullOrWhiteSpace(appConfigOptions.Endpoint))
        {
            options.Connect(appConfigOptions.Endpoint);
        }
    }
}
