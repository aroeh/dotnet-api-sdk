using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.API.SDK.Extensions;

public static class VersioningExtensions
{
    /// <summary>
    /// Adds API versioning
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="groupNameFormat">Format for group names from API versions.  If <see langword="null"/>, empty, or whitespace, defaults to "'v'VVV"</param>
    /// <returns><see cref="IServiceCollection"/> service collection</returns>
    public static IServiceCollection AddVersioning(this IServiceCollection services, string? groupNameFormat = null)
    {
        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = string.IsNullOrWhiteSpace(groupNameFormat) ? "'v'VVV" : groupNameFormat;
        });

        return services;
    }
}
