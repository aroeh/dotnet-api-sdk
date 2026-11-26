# Overview
Health Checks are useful for determining the health of your API and any services that it depends on.  For observability it is common for APIs to provide a health check endpoint to provide health information for monitoring.


## Health Check Options
The SDK uses the options pattern to read and map application settings into the `ApiHealthCheckOptions` and `ApiHealthRoute` classes.

Example app settings json
```json
"Api": {
  "Settings": {
    "HealthCheck": {
        "Routes": [
            {
                "Route": "/healthz",
                "Tags": []
            }
        ]
    }
  }
}
```

> Routes is an array, so multiple routes can be configured in app settings to enable setup of ready and live health endpoints.  The setup will show such an example.


## Custom Health Check Classes
There may be instances where you might need to write a custom health check for a service in your API.  Use the following code as an example for how to write such a class

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace MyAppNamespace;

public class MyCustomHealthCheck : IHealthCheck
{

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = new())
    {
        try
        {
            // TODO: Write code used to get health results of your service

            // TODO: Check the results to return health states
            if (myService is available)
            {
                // TODO: check for service degraded condition
                if (myService is available but not responding as expected)
                {
                    return HealthCheckResult.Degraded("MyService is Degraded", null, myServiceHealthResults.ToDictionary());
                }

                return HealthCheckResult.Healthy("MyService is Healthy", myServiceHealthResults.ToDictionary());
            }

            // TODO: Handle unhealthly condition
            return HealthCheckResult.Unhealthy("MyService is Not Healthy", null, myServiceHealthResults.ToDictionary());
        }
        catch (Exception ex)
        {
            // TODO: Handle unhealthly condition
            return HealthCheckResult.Unhealthy("MyService is Not Healthy", ex);
        }
    }
}
```


## Health Check Filters
By default, a health check endpoint will return the results of all registered health checks.  For some platforms, there may be a need to filter the results of the health checks.  The SDK enables registering and mapping endpoints using tags to filter the results.  Note that specifying a filter limits the results to only those matching filter values.

```csharp
using Dotnet.API.SDK.Extensions;

// register health checks using a custom class for a service
builder.Services.AddHealth<MyCustomHealthCheck>("MyRegistrationName", HealthStatus.UnHealthy, [ "myTag" ]);

var app = builder.Build();

app.UseHealthChecks("/healthz", [ "myTag" ]);
```

> The same tags must be used in both the registration and health check mapping.  The route and tag configuration may come from app settings or may be set explicitly.


## Readiness and Liveness Health Checks
All APIs must startup before they are ready to receive requests.  The amount of time between startup and readiness might vary by hosting platform and any tasks the API might need to run in order to be ready.  In such scenarios it might be useful to separate the health checks to look for two app states: Readiness and Liveness
* Readiness indicates the application is running but not yet ready to receive requests
* Liveness indicates if an application has crashed or is running normally

The SDK supports using and mapping multiple health check endpoints either via configuration options or explicitly mapping the endpoints.

Using configuration might look like the following:
```json
"Api": {
  "Settings": {
    "HealthCheck": {
        "Routes": [
            {
                "Route": "/healthz/ready",
                "Tags": [ "ready" ]
            },
            {
                "Route": "/healthz/live",
                "Tag": []
            }
        ]
    }
  }
}
```

Then using the extension methods
```csharp
using Dotnet.API.SDK.Extensions;

// register health checks using a custom class for a service
builder.Services.AddHealth<StartupHealthCheck>("Startup", null, [ "ready" ]);

var app = builder.Build();

app.UseHealthChecks(builder.Configuration);
```

Mapping the endpoints explicitly without using configuration settings
```csharp
using Dotnet.API.SDK.Extensions;

// register health checks using a custom class for a service
builder.Services.AddHealth<StartupHealthCheck>("Startup", null, [ "ready" ]);

var app = builder.Build();

app.UseHealthChecks("/healthz/ready", ["ready"]);
app.UseHealthChecks("/healthz/live");
```

### Startup Health Check
Out of the box, the `StartupHealthCheck` will return an unhealthy state.  In order to utilize it you must write code to update and set the `StartupCompleted` property on the class.

Take this simple background service for example
```csharp
public class StartupBackgroundService : BackgroundService
{
    private readonly StartupHealthCheck _healthCheck;

    public StartupBackgroundService(StartupHealthCheck healthCheck)
        => _healthCheck = healthCheck;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO: write startup tasks and remove the delay

        // Simulate the effect of a long-running task.
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        _healthCheck.StartupCompleted = true;
    }
}
```


## Setup
This setup example utilize app setting configuration to register and use health checks.

1. Add the following configuration settings to your application.  Use a secure vault or secrets.
```json
"Api": {
  "Settings": {
    "HealthCheck": {
        "Routes": [
            {
                "Route": "/healthz/ready",
                "Tags": [ "ready" ]
            },
            {
                "Route": "/healthz/live",
                "Tag": []
            }
        ]
    }
  }
}
```

> The platform for your API will determine which endpoints you may want to setup.  It is a common practice to have distinct endpoints indicating that the API is ready and for incremental liviness checks.


2. Register health check services on the builder services and map the endpoints, typically in `Program.cs`.
```csharp
using Dotnet.API.SDK.Extensions;

// register health checks using a custom class for a service
builder.Services.AddHealth<MyCustomHealthCheck>();

var app = builder.Build();

// map the health check endpoint and write responses using the HealthCheckResponseWriter middleware provided in the SDK
app.UseHealthChecks(builder.Configuration);
```
