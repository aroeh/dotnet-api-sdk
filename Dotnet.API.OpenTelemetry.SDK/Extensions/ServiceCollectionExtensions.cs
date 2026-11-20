using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

namespace Dotnet.API.OpenTelemetry.SDK.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddAppOpenTelemetry<TClass>(this WebApplicationBuilder app) where TClass : class
    {
        app.Services.AddOpenTelemetryServices<TClass>(app.Configuration);
        app.Logging.AddOpenTelemetryLogging<TClass>(app.Configuration);
    }

    public static void AddAppOpenTelemetry(this WebApplicationBuilder app, Assembly resourceAssembly)
    {
        app.Services.AddOpenTelemetryServices(app.Configuration, resourceAssembly);
        app.Logging.AddOpenTelemetryLogging(app.Configuration, resourceAssembly);
    }

    public static void AddOpenTelemetryServices<TClass>(this IServiceCollection services, IConfiguration configuration) where TClass : class
    {
        Assembly resourceAssembly = GetClassAssembly<TClass>();

        // Build a resource configuration action to set service information.
        var configureResource = GetResourceConfiguration(configuration, resourceAssembly);

        services.ConfigureTelemetryServices(configuration, configureResource);
    }

    public static void AddOpenTelemetryServices(this IServiceCollection services, IConfiguration configuration, Assembly resourceAssembly)
    {
        // Build a resource configuration action to set service information.
        var configureResource = GetResourceConfiguration(configuration, resourceAssembly);

        services.ConfigureTelemetryServices(configuration, configureResource);
    }

    public static void AddOpenTelemetryLogging<TClass>(this ILoggingBuilder logging, IConfiguration configuration) where TClass : class
    {
        Assembly resourceAssembly = GetClassAssembly<TClass>();
        var configureResource = GetResourceConfiguration(configuration, resourceAssembly);

        logging.ConfigureLogging(configureResource);
    }

    public static void AddOpenTelemetryLogging(this ILoggingBuilder logging, IConfiguration configuration, Assembly resourceAssembly)
    {
        var configureResource = GetResourceConfiguration(configuration, resourceAssembly);

        logging.ConfigureLogging(configureResource);
    }

    private static void ConfigureTelemetryServices(this IServiceCollection services, IConfiguration configuration, Action<ResourceBuilder> configureResource)
    {
        var aspInstrumentationConfig = configuration.GetSection("AspNetCoreInstrumentation");

        // Configure OpenTelemetry tracing and metrics with auto-start using the
        // AddOpenTelemetry() extension method from the OpenTelemetry.Extensions.Hosting package.
        services.AddOpenTelemetry()
            .ConfigureResource(configureResource)
            .UseOtlpExporter()
            .WithTracing(builder =>
            {
                builder
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();

                // Use IConfiguration binding for AspNetCore instrumentation options.
                services.Configure<AspNetCoreTraceInstrumentationOptions>(aspInstrumentationConfig);
            })
            .WithMetrics(builder =>
            {
                builder
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();
            });
    }

    private static void ConfigureLogging(this ILoggingBuilder logging, Action<ResourceBuilder> configureResource)
    {
        // Clear default logging providers used by WebApplication host.
        logging.ClearProviders();

        // Configure OpenTelemetry Logging.
        logging.AddOpenTelemetry(options =>
        {
            // See appsettings.json "Logging:OpenTelemetry" section for configuration.
            var resourceBuilder = ResourceBuilder.CreateDefault();
            configureResource(resourceBuilder);
            options.SetResourceBuilder(resourceBuilder);
        });
    }

    private static Assembly GetClassAssembly<TClass>() where TClass : class => typeof(TClass).Assembly;

    private static Action<ResourceBuilder> GetResourceConfiguration(IConfiguration configuration, Assembly resourceAssembly)
    {
        // TODO: Use Options pattern
        string serviceName = configuration.GetValue("ServiceName", defaultValue: "otel-test");
        string assemblyVersion = resourceAssembly.GetName().Version?.ToString() ?? "unknown";
        string serviceInstanceId = Environment.MachineName;

        // Build a resource configuration action to set service information.
        Action <ResourceBuilder> configureResource = r => r.AddService(
            serviceName: serviceName,
            serviceVersion: assemblyVersion,
            serviceInstanceId: serviceInstanceId
        );

        return configureResource;
    }
}
