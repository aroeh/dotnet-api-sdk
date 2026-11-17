using Dotnet.API.Redis.SDK.Interfaces;
using Dotnet.API.Redis.SDK.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace Dotnet.API.Redis.SDK.Repos;

public class RedisRepo(ILogger<RedisRepo> logger, IConnectionMultiplexer connectionMultiplexer, IOptions<RedisOptions> redisOptions) : IRedisRepo
{
    private readonly ILogger<RedisRepo> _logger = logger;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(redisOptions.Value.Expiration);
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(redisOptions.Value.Timeout);
    private readonly IDatabase _redisDb = connectionMultiplexer.GetDatabase();

    private readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Add a new value to cache using the specified key.  If the key exists, the value will be overwritten with the new value.
    /// </summary>
    /// <param name="key">Key used to identify the cache value.</param>
    /// <param name="value">Value to store in cache.</param>
    /// <remarks>
    /// <para>Values are stored as serialized json strings.</para>
    /// <para>Expiry is set using configured redis options.</para>
    /// </remarks>
    public void AddCacheValue(string key, object value)
    {
        AddCacheValue(key, value, _defaultExpiration);
    }

    /// <summary>
    /// Add a new value to cache using the specified key.  If the key exists, the value will be overwritten with the new value.
    /// </summary>
    /// <param name="key">Key used to identify the cache value.</param>
    /// <param name="value">Value to store in cache.</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Task adding a value to cache</returns>
    public async Task AddCacheValueAsync(string key, object value, CancellationToken cancellationToken)
    {
        await AddCacheValueAsync(key, value, _defaultExpiration, cancellationToken);
    }

    /// <summary>
    /// Add a new value to cache using the specified key.  If the key exists, the value will be overwritten with the new value.
    /// </summary>
    /// <param name="key">Key used to identify the cache value.</param>
    /// <param name="value">Value to store in cache.</param>
    /// <param name="expiration">Cache value expiration overriding the configured default expiration.</param>
    public void AddCacheValue(string key, object value, TimeSpan expiration)
    {
        _logger.LogInformation("Adding new cache value. Key: {cacheKey} | Expires: {expiry}", key, DateTime.UtcNow.AddMinutes(expiration.Minutes));
        string cacheString = JsonSerializer.Serialize(value, JsonSerializerOptions);
        
        _redisDb.StringSet(key, cacheString, expiration);
    }

    /// <summary>
    /// Add a new value to cache using the specified key.  If the key exists, the value will be overwritten with the new value.
    /// </summary>
    /// <param name="key">Key used to identify the cache value.</param>
    /// <param name="value">Value to store in cache.</param>
    /// <param name="expiration">Cache value expiration overriding the configured default expiration.</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Task adding a value to cache</returns>
    public async Task AddCacheValueAsync(string key, object value, TimeSpan expiration, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding new cache value. Key: {cacheKey} | Expires: {expiry}", key, DateTime.UtcNow.AddMinutes(expiration.Minutes));
        string cacheString = JsonSerializer.Serialize(value, JsonSerializerOptions);

        await _redisDb.StringSetAsync(key, cacheString, expiration).WaitAsync(_timeout, cancellationToken);
    }

    /// <summary>
    /// Retrieves a value from cache using the specified key
    /// </summary>
    /// <typeparam name="T">Object type instance of the cached value.</typeparam>
    /// <param name="key">Key used to identify the cache value.</param>
    /// <returns>Instance of specified type if it exists in cache, else returns null.</returns>
    public T? GetCacheValue<T>(string key)
    {
        _logger.LogInformation("Retrieving cache value for key: {cacheKey}", key);
        string? cacheString = _redisDb.StringGet(key);
        return string.IsNullOrWhiteSpace(cacheString)
            ? default(T)
            : JsonSerializer.Deserialize<T>(cacheString, JsonSerializerOptions);
    }

    /// <summary>
    /// Retrieves a value from cache using the specified key
    /// </summary>
    /// <typeparam name="T">Object type instance of the cached value.</typeparam>
    /// <param name="key">Key used to identify the cache value.</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Task that returns a value from cache if exists and null if not.</returns>
    public async Task<T?> GetCacheValueAsync<T>(string key, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving cache value for key: {cacheKey}", key);
        string? cacheString = await _redisDb.StringGetAsync(key).WaitAsync(_timeout, cancellationToken);
        return string.IsNullOrWhiteSpace(cacheString)
            ? default(T)
            : JsonSerializer.Deserialize<T>(cacheString, JsonSerializerOptions);
    }

    /// <summary>
    /// Removes the cached value for the specified key.
    /// </summary>
    /// <param name="key">Key used to identify the cache value.</param>
    public void RemoveCacheValue(string key)
    {
        if (_redisDb.KeyExists(key))
        {
            _logger.LogInformation("Removing cache value for key: {cacheKey}", key);
            _redisDb.StringGetDelete(key);
        }
    }

    /// <summary>
    /// Removes the cached value for the specified key.
    /// </summary>
    /// <param name="key">Key used to identify the cache value.</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Task removing a value from cache</returns>
    public async Task RemoveCacheValueAsync(string key, CancellationToken cancellationToken)
    {
        if (await _redisDb.KeyExistsAsync(key).WaitAsync(_timeout, cancellationToken))
        {
            _logger.LogInformation("Removing cache value for key: {cacheKey}", key);
            await _redisDb.StringGetDeleteAsync(key).WaitAsync(_timeout, cancellationToken);
        }
    }
}
