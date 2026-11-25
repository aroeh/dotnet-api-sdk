# Overview
This SDK offers basic tools for registering Entity Framework Core against Microsoft SQL Server and a generic repository for CRUD operations. It binds a connection string, registers a `DbContext`, and provides create, query, update, and delete methods for entities that inherit `EntityBase`.


# Extensions

## MSSql Options
There is 1 required property the SDK uses for SQL Server:
1. ConnectionString

The connection string indicates which SQL Server database to connect to.

`AddEFCoreDbContext` throws on application startup if the options section is missing or `ConnectionString` is null, empty, or whitespace. `ConfigureMSSqlOptions` throws if the configuration section is missing.


## ConfigureMSSqlOptions
This method adds options to the application service collection so they can be injected. The default configuration key is `Api:Settings:MSSql`.

Usage:
On application startup, typically in Program.cs call the method with the builder Services.
```csharp
using Dotnet.API.MSSQL.SDK.Extensions;

builder.Services.ConfigureMSSqlOptions(builder.Configuration);
```

The above example reads `ConnectionString` from `Api:Settings:MSSql`.

A custom configuration key for settings may be used as well.
```csharp
using Dotnet.API.MSSQL.SDK.Extensions;

builder.Services.ConfigureMSSqlOptions(builder.Configuration, "mycustomConfigKey");
```

Inject options into a service:
```csharp
using Dotnet.API.MSSQL.SDK.Options;
using Microsoft.Extensions.Options;

public class MyRepo : IMyRepo
{
    private readonly IOptions<MSSqlOptions> _sqlOptions;

    public MyRepo(IOptions<MSSqlOptions> sqlOptions)
    {
        _sqlOptions = sqlOptions;
    }

    // ... class methods and properties
}
```


## AddEFCoreDbContext
This method registers an application `DbContext` with the SQL Server provider. It reads `MSSqlOptions` from configuration and calls `UseSqlServer` with `ConnectionString`.

The context type is the application's `DbContext`. Register it before registering `EFSqlRepo`, because the repository depends on that context.

Usage:
```csharp
using Dotnet.API.MSSQL.SDK.Extensions;

builder.Services.AddEFCoreDbContext<MyDbContext>(builder.Configuration);
```

A custom configuration key may be passed as well.
```csharp
builder.Services.AddEFCoreDbContext<MyDbContext>(builder.Configuration, "mycustomConfigKey");
```


## AddScopedEFSqlRepo or AddTransientEFSqlRepo
`EFSqlRepo` must be registered for use in the application service collection. Extension methods register it as scoped or transient. Select the one that fits the application pipeline and scope of work.

Registration is generic over the `DbContext` and the entity. The entity must inherit `EntityBase`, which supplies a string `Id`.

Usage:
```csharp
using Dotnet.API.MSSQL.SDK.Extensions;

builder.Services.AddScopedEFSqlRepo<MyDbContext, MyEntity>();
```

Use `AddTransientEFSqlRepo<MyDbContext, MyEntity>()` when a new repository should be created for each request of the service.


## EFSqlRepo
`EFSqlRepo` provides a generic repository for basic CRUD against a SQL Server database through Entity Framework Core. It offers the following features:
1. Create one entity and return it
2. Create many entities
3. Query entities from an `IQueryable`, or with an optional filter
4. Query a page of entities with offset pagination
5. Get one entity by id, with an optional additional filter
6. Update many entities
7. Remove one entity, or many entities by id
8. Save pending changes on the `DbContext`

`QueryBase` exposes the entity `IQueryable` for composing filters in application code. Pass that query to the query methods.

Paginated queries use offset pagination. The default page is 1 and the default page size is 25. Results are ordered by `Id`. Pagination can be passed as page and page size, or as `PaginationQueryParametersBO`. Create, update, and delete methods that affect a set of rows return `TransactionResult`.

Use `UpdateManyAsync` to update entities.


<!-- ## ADO.NET
`ADOSqlRepo` and `ADOSqlRepoBase` contain helpers for stored procedures and SQL text commands, including row reads, scalar execution, and non-query execution. The schema defaults to `dbo`. Those helpers are internal, and no extension method registers `IADOSqlRepo`.

Use the Entity Framework methods in this guide to register SQL Server in an application. `ConfigureMSSqlOptions` is available when application code needs the connection string through `IOptions<MSSqlOptions>`. -->


# Setup
1. Add an entity that inherits `EntityBase`, and a `DbContext` with a `DbSet` for that entity.
```csharp
using Dotnet.API.MSSQL.SDK.Entities;
using Microsoft.EntityFrameworkCore;

public class MyEntity : EntityBase
{
    public string Name { get; set; } = string.Empty;
}

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    public DbSet<MyEntity> MyEntities => Set<MyEntity>();
}
```

2. Add the following configuration settings to your application. Use a secure vault or secrets.
```json
"Api": {
  "Settings": {
    "MSSql": {
      "ConnectionString": "Your-connection-string"
    }
  }
}
```

> You can use your own custom object structure/path to the connection string. Pass that configuration key when registering options and the DbContext in the next steps.

3. Add the configuration to the services collection, typically in `Program.cs`.
```csharp
using Dotnet.API.MSSQL.SDK.Extensions;

builder.Services.ConfigureMSSqlOptions(builder.Configuration);
```

4. Register the DbContext and the EF SQL repository.
```csharp
using Dotnet.API.MSSQL.SDK.Extensions;

builder.Services.AddEFCoreDbContext<MyDbContext>(builder.Configuration);
builder.Services.AddScopedEFSqlRepo<MyDbContext, MyEntity>();
```

5. Add your application repos.
```csharp
builder.Services.AddScoped<IMyRepo, MyRepo>();
```

6. Inject `IEFSqlRepo` into your repo.
```csharp
using Dotnet.API.MSSQL.SDK.Interfaces;

namespace MyApp.AppNamespace;

public class MyRepo
(
    ILogger<MyRepo> logger,
    IEFSqlRepo<MyDbContext, MyEntity> sqlRepo
) : IMyRepo
{
    private readonly ILogger<MyRepo> _logger = logger;
    private readonly IEFSqlRepo<MyDbContext, MyEntity> _sqlRepo = sqlRepo;

    // TODO: Repo methods go here...
}
```
