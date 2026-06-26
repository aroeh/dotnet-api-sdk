using Dotnet.API.SDK.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.API.SDK.Extensions;

public static class ExceptionExtensions
{
    /// <summary>
    /// Adds the common GlobalExceptionHandler and registers problem details
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns><see cref="IServiceCollection"/> service collection</returns>
    public static IServiceCollection AddExceptionHandlerAndProblemDetails(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    /// <summary>
    /// Adds a custom exception handler and registers problem details
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns><see cref="IServiceCollection"/> service collection</returns>
    public static IServiceCollection AddExceptionHandlerAndProblemDetails<T>(this IServiceCollection services) where T : class, IExceptionHandler
    {
        services.AddExceptionHandler<T>();
        services.AddProblemDetails();

        return services;
    }
}
