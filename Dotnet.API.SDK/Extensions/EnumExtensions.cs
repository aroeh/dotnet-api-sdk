using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace Dotnet.API.SDK.Extensions;

// TODO: look into the practicality of this one
public static class EnumExtensions
{
    /// <summary>
    /// Adds the common GlobalExceptionHandler and registers problem details
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns><see cref="IServiceCollection"/> service collection</returns>
    public static IServiceCollection AddJsonStringEnumConverter(this IServiceCollection services)
    {
        //services.AddJsonOptions(options => // handle enum json serialization for incoming requests
        //{
        //    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        //});

        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}
