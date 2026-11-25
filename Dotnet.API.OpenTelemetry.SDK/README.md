# Overview
This SDK registers OpenTelemetry for an ASP.NET Core API. It configures a service resource, exports traces, metrics, and logs with the OTLP exporter, and turns on ASP.NET Core and HttpClient instrumentation.


# Extensions

## Configuration
The SDK does not use an options class. It reads values from application configuration and from the standard OpenTelemetry environment variables.

### ServiceName
`ServiceName` is the service name added to the OpenTelemetry resource. It is read from the root of configuration.

If `ServiceName` is missing, the SDK uses `otel-test`.

The resource also includes:
1. Service version, taken from the assembly version of the type or assembly passed at registration. If the assembly has no version, the value is `unknown`.
2. Service instance id, set to the machine name.

### Logging:OpenTelemetry
OpenTelemetry logging options are read from the `Logging:OpenTelemetry` section:

1. IncludeFormattedMessage
2. IncludeScopes
3. ParseStateValues

Set each value to `true` when log exports should include the formatted message, logging scopes, and structured state values.

### AspNetCoreInstrumentation
The `AspNetCoreInstrumentation` section is bound to `AspNetCoreTraceInstrumentationOptions` for ASP.NET Core tracing.

`RecordException` records request exceptions on the span when set to `true`.

### OTLP exporter
Traces, metrics, and logs are sent with `UseOtlpExporter()`. The exporter reads the standard OpenTelemetry environment variables:

1. `OTEL_EXPORTER_OTLP_ENDPOINT` is the collector or backend endpoint.
2. `OTEL_EXPORTER_OTLP_PROTOCOL` is the protocol, such as `http/protobuf`.
3. `OTEL_METRIC_EXPORT_INTERVAL` is how often metrics are exported, in milliseconds.

Set these in the environment, user secrets, or the launch profile. Signals are not exported until an endpoint is configured.


## AddAppOpenTelemetry
This method registers tracing, metrics, and logging on a `WebApplicationBuilder`. It is the usual registration for an API.

The generic overload uses the assembly of `TClass` for the service version.

Usage:
On application startup, typically in Program.cs:
```csharp
using Dotnet.API.OpenTelemetry.SDK.Extensions;

builder.AddAppOpenTelemetry<Program>();
```

An assembly can be passed instead:
```csharp
using Dotnet.API.OpenTelemetry.SDK.Extensions;
using System.Reflection;

var assembly = Assembly.GetExecutingAssembly();
builder.AddAppOpenTelemetry(assembly);
```

`AddAppOpenTelemetry` does two things:
1. Calls `AddOpenTelemetryServices` to register tracing and metrics.
2. Calls `AddOpenTelemetryLogging` to send logs through OpenTelemetry.

Logging registration clears the default logging providers. Add any other providers after this call if logs should also go to the console or another provider.


## AddOpenTelemetryServices
This method registers tracing and metrics only. Use it when logging is configured separately.

Tracing includes HttpClient and ASP.NET Core instrumentation. Metrics include the same two instrumentations. Both use the service resource described above, and traces and metrics are exported with the OTLP exporter.

Usage:
```csharp
using Dotnet.API.OpenTelemetry.SDK.Extensions;

builder.Services.AddOpenTelemetryServices<Program>(builder.Configuration);
```

Or pass an assembly:
```csharp
using Dotnet.API.OpenTelemetry.SDK.Extensions;

builder.Services.AddOpenTelemetryServices(builder.Configuration, assembly);
```


## AddOpenTelemetryLogging
This method registers OpenTelemetry logging only. It clears the default logging providers, then adds the OpenTelemetry logger with the same service resource used for traces and metrics.

Logging options come from the `Logging:OpenTelemetry` configuration section.

Usage:
```csharp
using Dotnet.API.OpenTelemetry.SDK.Extensions;

builder.Logging.AddOpenTelemetryLogging<Program>(builder.Configuration);
```

Or pass an assembly:
```csharp
using Dotnet.API.OpenTelemetry.SDK.Extensions;

builder.Logging.AddOpenTelemetryLogging(builder.Configuration, assembly);
```


# Setup
1. Add the service and instrumentation settings to your application.
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    },
    "OpenTelemetry": {
      "IncludeFormattedMessage": true,
      "IncludeScopes": true,
      "ParseStateValues": true
    }
  },
  "ServiceName": "my-api",
  "AspNetCoreInstrumentation": {
    "RecordException": true
  }
}
```

2. Set the OTLP exporter environment variables on your API.
```json
"environmentVariables": {
  "OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4318",
  "OTEL_EXPORTER_OTLP_PROTOCOL": "http/protobuf",
  "OTEL_METRIC_EXPORT_INTERVAL": "5000"
}
```

3. Register OpenTelemetry on the application builder, typically in `Program.cs`.
```csharp
using Dotnet.API.OpenTelemetry.SDK.Extensions;
using System.Reflection;

var assembly = Assembly.GetExecutingAssembly();
builder.AddAppOpenTelemetry(assembly);
```

4. Log through the normal ASP.NET Core `ILogger`. Those logs are exported with the service resource once the OTLP endpoint is set.
```csharp
app.Logger.LogInformation("Running application");
```
