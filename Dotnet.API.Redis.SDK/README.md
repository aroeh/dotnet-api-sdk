# Overview
This SDK offers basic tools for registering a generic repository for Redis caching operations.  It enables connecting, adding, retrieving, and removing cached data in Redis databases.


# Extensions

## Redis Options
There are 4 required properties the SDK uses for Redis:
1. Expiration
2. Timeout
3. Host
4. Port

* Expiration specifies in minutes how long a value will remain cached
* Timeout specified in seconds, async operations should wait before timing out
* Host refers to the server or instance where the redis data is accessible
* Port is the connection to use on the host.  The default port for Redis is 6379

If any property is missing, an exception will be thrown on application startup.


## ConfigureRedisOptions
This method adds options to the application service collection to be injected in the application.  The options may be useful if using a custom repo or connection method outside of the SDK.

Usage:
On application startup, typically in Program.cs call the method with the builder Services.
```csharp
using Dotnet.API.Redis.SDK.Extensions;

builder.Services.ConfigureRedisOptions(builder.Configuration);
```

The above example will use the default configuration key of `Api:Settings:Redis` to read Expiration, Timeout, Host and Port properties.

A custom configuration key for settings may be used as well
```csharp
using Dotnet.API.Redis.SDK.Extensions;

builder.Services.ConfigureRedisOptions(builder.Configuration, "mycustomConfigKey");
```

Inject options into a service
```csharp
using Dotnet.API.Redis.SDK.Options;

public class MyRepo() : IMyRepo
{
    private readonly IOptions<RedisOptions> _redisOptions;
    
    public MyRepo(IOptions<RedisOptions> redisOptions)
    {
        _redisOptions = redisOptions;
    }

    // ... class methods and properties
}
```


## AddRedisConnectionMultiplexer
Redis has the option to use a connection multiplexor to manage connections to distributed cache databases.  This method adds a singleton instance of the connection multiplexor.  Use this method if you want to have control on where to inject and access the connection within custom repos or other app services.

Usage:
On application startup, typically in Program.cs call the method with the builder Services.
```csharp
using Dotnet.API.Redis.SDK.Extensions;

builder.Services.AddRedisConnectionMultiplexer(builder.Configuration);
```

The above example will use the default configuration key of `Api:Settings:Redis` to read Expiration, Timeout, Host and Port properties.

A custom configuration key for settings may be used as well
```csharp
using Dotnet.API.Redis.SDK.Extensions;

builder.Services.AddRedisConnectionMultiplexer(builder.Configuration, "mycustomConfigKey");
```

Inject `IConnectionMultiplexer` into a service and get the database for cache operations
```csharp
using StackExchange.Redis;

public class MyRepo() : IMyRepo
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _redisDb;
    
    public MyRepo(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _redisDb = _connectionMultiplexer.GetDatabase();
    }

    // ... class methods and properties
}
```

## AddRedisScopedServices
This is a convenience method that handles registering necessary Redis services within the application and providing a repository for caching operations.  This method does the following"
* Registers an instance of `RedisOptions` in the application service collection
* Adds the Redis `IConnectionMultiplexor` to the application service collection
* Adds a scoped instance of the `IRedisRepo` and `RedisRepo` to the service collection

The Redis repo depends on the options and connection multiplexor being registered, so this method ensures that all services are added.

Usage:
On application startup, typically in Program.cs call the method with the builder Services.
```csharp
using Dotnet.API.Redis.SDK.Extensions;

builder.Services.AddRedisScopedServices(builder.Configuration);
```

The above example will use the default configuration key of `Api:Settings:Redis` to read Expiration, Timeout, Host and Port properties.

A custom configuration key for settings may be used as well
```csharp
using Dotnet.API.Redis.SDK.Extensions;

builder.Services.AddRedisScopedServices(builder.Configuration, "mycustomConfigKey");
```

Inject `IRedisRepo` into a service and use cache operations
```csharp
using StackExchange.Redis;

public class MyRepo() : IMyRepo
{
    private readonly IRedisRepo _redis;
    
    public MyRepo(IRedisRepo redis)
    {
        _redis = redis;
    }

    public async Task<object> GetMyObjectAsync(string id, CancellationToken cancellationToken)
    {
        object? cache = await _redis.GetCacheValueAsync<RestuarantBO>(id, cancellationToken);
        if (cache is not null)
        {
            return cache;
        }

        // ... get from the DB if not found in cache
    }
}
```

## AddRedisTransientServices
This is a convenience method that handles registering necessary Redis services within the application and providing a repository for caching operations.  This method does the following"
* Registers an instance of `RedisOptions` in the application service collection
* Adds the Redis `IConnectionMultiplexor` to the application service collection
* Adds a transient instance of the `IRedisRepo` and `RedisRepo` to the service collection

The Redis repo depends on the options and connection multiplexor being registered, so this method ensures that all services are added.

Usage:
On application startup, typically in Program.cs call the method with the builder Services.
```csharp
using Dotnet.API.Redis.SDK.Extensions;

builder.Services.AddRedisTransientServices(builder.Configuration);
```

The above example will use the default configuration key of `Api:Settings:Redis` to read Expiration, Timeout, Host and Port properties.

A custom configuration key for settings may be used as well
```csharp
using Dotnet.API.Redis.SDK.Extensions;

builder.Services.AddRedisTransientServices(builder.Configuration, "mycustomConfigKey");
```

Inject `IRedisRepo` into a service and use cache operations
```csharp
using StackExchange.Redis;

public class MyRepo() : IMyRepo
{
    private readonly IRedisRepo _redis;
    
    public MyRepo(IRedisRepo redis)
    {
        _redis = redis;
    }

    public async Task<object> GetMyObjectAsync(string id, CancellationToken cancellationToken)
    {
        object? cache = await _redis.GetCacheValueAsync<RestuarantBO>(id, cancellationToken);
        if (cache is not null)
        {
            return cache;
        }

        // ... get from the DB if not found in cache
    }
}
```

## RedisRepo
The `RedisRepo` provides a generic interface for handling basic operations when interacting with a Redis Database.  It offers the following features:
1. Add values to cache, optionally allowing an override of the default expiration in minutes
2. Get a value from cache
3. Remove a value from cache

Both synchronous and asynchronous are available.  The asynchronous methods utilize the cancellation token and default timeout on RedisOptions.


# Setup
1. Add the following configuration settings to your application.  Use a secure vault or secrets.
```json
"Api": {
  "Settings": {
    "Redis": {
        "Expiration": "10", // minutes when the cache value will expire
        "Timeout": "3", // seconds for operations to wait before timing out
        "Host": "Your-host-name",
        "Port": "6379"
    }
  }
}
```

> You can use your own custom object structure/path to the connection string and database name properties.  Just pass in the configuration key when registering the options in step 2

2. Add the Redis scoped services to the services collection, typically in `Program.cs`
```csharp
using Dotnet.API.Redis.SDK.Extensions;

builder.Services.AddRedisScopedServices(builder.Configuration);
```

3. Add your application repos or other classes
```csharp
builder.Services.AddScoped<IMyRepo, MyRepo>();
```

4. Inject the `RedisRepo` into your repo
```csharp
using Dotnet.API.Redis.SDK.Interfaces;

namespace MyApp.AppNamespace;

public class MyRepo
(
    ILogger<MyRepo> log,
    IRedisRepo redis
) : IMyRepo
{
    private readonly ILogger<MyRepo> _logger = log;
    private readonly IRedisRepo _redis = redis;

    // TODO: Repo methods go here...
}
```