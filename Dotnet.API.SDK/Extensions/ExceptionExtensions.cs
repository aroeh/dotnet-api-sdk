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
        Console.Write("Adding exception handler and problem details...");

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        Console.WriteLine("Complete");

        return services;
    }

    /// <summary>
    /// Adds a custom exception handler and registers problem details
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns><see cref="IServiceCollection"/> service collection</returns>
    public static IServiceCollection AddExceptionHandlerAndProblemDetails<T>(this IServiceCollection services) where T : class, IExceptionHandler
    {
        Console.Write("Adding exception handler and problem details...");

        services.AddExceptionHandler<T>();
        services.AddProblemDetails();

        Console.WriteLine("Complete");

        return services;
    }
}
