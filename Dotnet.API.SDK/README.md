# Overview
This SDK offers services commonly used in APIs.  Services are all provided from a common project and can be added flexibly.  The goal of this API is reduce the need to write common middleware and code connecting and injecting common services.


# Features
1. Base API controller to provide a common logging property and common API service tags
2. Exception Handling
    * Global exception handling and problem details to catch errors in the API and return a response
    * Custom exception interface to enable extending your app to add more custom exceptions which will automatically be useable by the Global Exception Handler
    * A custom NotFoundException to handle scenarios where data is not found but was expected to exist
3. Extensions to register common services
    * Azure App Configuration to connect to the Azure App Config service and get values by label and key filters
    * Configuration Extensions to read appsettings.json files
    * Exception Extensions to register a global exception handler and add problem details
    * Health Check Extensions to register a health endpoint from configuration settings or by specifying a custom route
    * API Versioning - currently by query string parameters
4. Health Check Response Writer middleware
5. Common API Models
    * Pagination Query Parameters for API endpoints that utilize pagination
6. ID Generator Utility if your application needs to generate IDs for entities before writing to a DB



## ApiControllerBase
Usage:
On application startup, typically in Program.cs call the method with the builder Services.
```csharp
using Dotnet.API.SDK.Controllers;

public class MyApiController
(
    ILogger<MyApiController> logger
) : ApiControllerBase<MyApiController>(logger)
{
    // ... API endpoints and actions
}
```


## Global Exception Handling
Usage:
On application startup, typically in Program.cs call the method with the builder Services.
```csharp
using Dotnet.API.SDK.Extensions;

// register the global exception handler middleware
builder.Services.AddExceptionHandlerAndProblemDetails();

var app = builder.Build();

// instruct the application to use the exception handling middleware
app.UseExceptionHandler();
```


## Not Found Exception
The `NotFoundException` is useful when you need to validate that data exists before processing further.  For example, when updating an entity you it is beneficial to check if the entity can be found before attempting an update operation.

Usage:
```csharp
using Dotnet.API.SDK.Exceptions;

public async Task<bool> UpdateMyDataAsync(string id, object request, CancellationToken cancellationToken)
{
    _ = await GetMyDataAsync(id, cancellationToken) ?? throw new NotFoundException("Data does not exist.  Unable to update.", id);

    // ... update data logic
}
```


## ICustomException
The `ICustomException` is provided to simplify setup of extending the application to add more custom exceptions.  By adding the interface implementation to a new exception class, it will automatically be available for use by the API SDKs provided global exception middleware

Usage:
```csharp
using Dotnet.API.SDK.Exceptions;

namespace MyAppNamespace;

public class MyCustomException : Exception, ICustomException
{
    public int StatusCode => (int)HttpStatusCode.NotFound;
    public string Title => "My Exception Title";
    public string DataKey => "MyExceptionDataKey";

    // TODO: Add Custom Exception Properties
    public string? MyCustomExceptionProperty { get; }


    public MyCustomException(string msg)
        : base(msg) { }

    public MyCustomException(string msg, Exception innerException)
        : base(msg, innerException) { }

    // TODO: Use the custom properties within constructors to allow callers to pass in additional data for problem details
    public MyCustomException(string msg, string customProperty)
        : this(msg)
    {
        MyCustomExceptionProperty = customProperty;
        Data.Add(DataKey, MyCustomExceptionProperty);
    }

    public MyCustomException(string msg, Exception innerException, string customProperty)
        : this(msg, innerException)
    {
        MyCustomExceptionProperty = customProperty;
        Data.Add(DataKey, MyCustomExceptionProperty);
    }
}
```


## Configuration Extensions
Dotnet APIs will read configuration values from several locations: appsettings.json, environment variables, and user secrets.  The code for reading configuration from each of these locations is common.  The SDK provides convenience methods for reading configuration values.

There are 3 different ways to add configuration to the builder configuration.

1. Use only the ConfigurationManager:
```csharp
using Dotnet.API.SDK.Extensions;

builder.Configuration.AddConfigSettings();

var app = builder.Build();
```

2. Use a class within an assembly to read configuration:
```csharp
using Dotnet.API.SDK.Extensions;

builder.Configuration.AddConfigSettings<Program>();

var app = builder.Build();
```

3. Use an assembly to read configuration:
```csharp
using Dotnet.API.SDK.Extensions;

builder.Configuration.AddConfigSettings(Assembly.GetExecutingAssembly());

var app = builder.Build();
```


## Azure App Configuration Extensions
Azure App Configuration is a cloud service that can be used to securely store and retrieve application configuration settings.  It enables centralizing configuration settings for all applications and developers and if used properly can minimize the chances of secrets being exposed in code.  The SDK provides extensions for connecting to Azure App Configuration service and retrieving configuration values.

1. Add the following to secrets:
```json
"AppConfiguration": {
  "Endpoint": "my-azure-app-endpoint",
  "ConnectionString": "my-azure-app-connection-string"
}
```

> Only one of the Endpoint or ConnectionString properties needs to be set.  If both are set, the ConnectionString will be used.

2. Add Azure App Config to the builder configuration:
```csharp
using Dotnet.API.SDK.Extensions;

// get additional app settings from Azure App Configuration using the environment as a label filter
builder.Configuration.AddAzureAppConfigSettings([builder.Environment.EnvironmentName]);

var app = builder.Build();
```


## API Versioning
API Versioning is useful when adding features to APIs without impacting current users.  New features can be safely added on a new version of the API without impacting the previous version.  Versioning does require a more in-depth plan for handling versioned code in your solution and a maturity plan on deprecating and removing old versions.  The SDK provides the base functionality for adding API versioning to your endpoints.

1. Add versioning to the builder services collection.
```csharp
using Dotnet.API.SDK.Extensions;

// Add API versioning to the controllers.  By default it is configured for query string versioning
// and requires the query string param api-version on calls to the API.
builder.Services.AddVersioning();

var app = builder.Build();
```

2. Optional: Add a version class to help manage versions
```csharp
namespace MyAppNamespace;

public static class ApiVersions
{
    public const string Latest = V1;
    public const string QueryParamVersion = $"?api-version={Latest}";

    public const string V1 = "1.0";

    public static string[] AllVersions => [
        V1
    ];
}
```

3. Add the ApiVersion attribute to your controllers
```csharp
using Asp.Versioning;

namespace MyAppNamespace;

// Optionally you can use the ApiVersions class instead of hard-coding the version
// [ApiVersion(ApiVersions.Latest)]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class MyApiControllerController()
{
}
```

4. Include `api-version` query parameter on http requests
```http
GET /api/myController?api-version=1.0&page=1&pageSize=25 HTTP/1.1
Host: localhost:5017
```


## PaginationQueryParameters
The PaginationQueryParameters model is provided on the API SDK to provide a common model for accepting common pagination requests.  Page and PageSize are optional.  If used together with the PaginationQueryParametersBO model, the page and page size will default to 1 and 25 respectively.

Usage:
```csharp
using Dotnet.API.SDK.Models;
using Microsoft.AspNetCore.Mvc;

namespace MyAppNamespace;

public record MyDataQueryParameters : PaginationQueryParameters
{
    [FromQuery(Name = "myDataProperty")]
    public string[]? MyDataProperty { get; init; } = default!;
}
```


## IdGenerator
The IdGenerator is a convenience class provided to generate entity ids.  This can be useful when writing to multiple different databases, ex: SQL and MongoDb.  This would most likely be used in an orchestrator class that handles calls to write data to databases to ensure that the same id is generated for entity data.

Usage:
```csharp
using Dotnet.API.SDK.Utils;

public async Task<object> CreateMyDataAsync(object request, CancellationToken cancellationToken)
{
    _logger.LogInformation("Adding new data");
    string newId = IdGenerator.GenerateId();
    
    // ... handle writing data

    return newData;
}
```
