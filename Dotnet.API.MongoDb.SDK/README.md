# Overview
This SDK offers basic tools for registering a generic repository for MongoDb operations.  It enables querying, creating, updating, and deleting data in MongoDb database collections.


# Extensions

## MongoDb Options
There are 2 required properties the SDK uses for MongoDb:
1. ConnectionString
2. DatabaseName

The connection string indicates which MongoDb cluster to connect to.  The DatabaseName specifies which database in the cluster will be targeted for CRUD operations.

If either property is missing, an exception will be thrown on application startup.


## ConfigureMongoDbOptions
This method adds options to the application service collection to be injected in the application.  The MongoDbRepo requires the options. in order to connect to the database.

Usage:
On application startup, typically in Program.cs call the method with the builder Services.
```csharp
using Dotnet.API.MongoDb.SDK.Extensions;

builder.Services.ConfigureMongoDbOptions(builder.Configuration);
```

The above example will use the default configuration key of Api:Settings:MongoDb to read ConnectionString and DatabaseName properties.

A custom configuration key for settings may be used as well
```csharp
using Dotnet.API.MongoDb.SDK.Extensions;

builder.Services.ConfigureMongoDbOptions(builder.Configuration, "mycustomConfigKey");
```


## MongoDbRepo
The MongoDbRepo provides a generic interface for handling basic CRUD operations when interacting with a database collection.  It offers the following features:
1. Get all documents within a collection
2. Get a paginated collection of documents within a collection
3. Retrieve a document using a FilterDefinition
4. Create one or many new documents in a collection
5. Replace one document in a collection
6. Update one document in a collection
7. Delete one or many documents in a collection

The class is registered for dependency injection with an entity.  The MongoDb repo will handle serialization for the assigned class automatically.

## AddScopedMongoDbRepo or AddTransientMongoDbRepo
The MongoDbRepo must be registered for use in the application service collection.  There are extension methods that assist with registering the class for Scoped or Transient scopes.  Select the one that fits your application pipeline and scope of work.

For DI registration, specify the name of the collection for the paired entity/document.

Usage:
```csharp
using Dotnet.API.MongoDb.SDK.Extensions;

builder.Services.AddScopedMongoDbRepo("myCollectionName");
```

# Setup
1. Add the following configuration settings to your application.  Use a secure vault or secrets.
```json
"Api": {
  "Settings": {
    "MongoDb": {
        "ConnectionString": "Your-connection-string",
        "DatabaseName": "YourDatabaseName"
    }
  }
  
}
```

> You can use your own custom object structure/path to the connection string and database name properties.  Just pass in the configuration key when registering the options in step 2

2. Add the configuration to the services collection, typically in `Program.cs`
```csharp
using Dotnet.API.MongoDb.SDK.Extensions;

builder.Services.ConfigureMongoDbOptions(builder.Configuration);
```

3. Add the MongoDbRepo to the services collection
```csharp
using Dotnet.API.MongoDb.SDK.Extensions;

builder.Services.AddScopedMongoDbRepo("myCollectionName");
```

4. Add your application repos
```csharp
builder.Services.AddScoped<IMyRepo, MyRepo>();
```

5. Inject the MongoDbRepo into your repo
```csharp
using Dotnet.API.MongoDb.SDK.Interfaces;

namespace Demo.Restuarants.Infrastructure.MongoDb.Repos;

public class MyRepo
(
    ILogger<MyRepo> log,
    IMongoDbRepo<MyDocument> mongoDbRepo
) : IMyRepo
{
    private readonly ILogger<MyRepo> _logger = log;
    private readonly IMongoDbRepo<MyDocument> _mongoDbRepo = mongoDbRepo;

    // TODO: Repo methods go here...
}
```