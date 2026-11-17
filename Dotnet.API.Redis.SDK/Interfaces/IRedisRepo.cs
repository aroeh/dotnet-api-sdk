namespace Dotnet.API.Redis.SDK.Interfaces;

public interface IRedisRepo
{
    /// <summary>
    /// Add a new value to cache using the specified key.  If the key exists, the value will be overwritten with the new value.
    /// </summary>
    /// <param name="key">Key used to identify the cache value.</param>
    /// <param name="value">Value to store in cache.</param>
    /// <remarks>
    /// <para>Values are stored as serialized json strings.</para>
    /// <para>Expiry is set using configured redis options.</para>
    /// </remarks>
    void AddCacheValue(string key, object value);

    /// <summary>
    /// Add a new value to cache using the specified key.  If the key exists, the value will be overwritten with the new value.
    /// </summary>
    /// <param name="key">Key used to identify the cache value.</param>
    /// <param name="value">Value to store in cache.</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Task adding a value to cache</returns>
    Task AddCacheValueAsync(string key, object value, CancellationToken cancellationToken);

    /// <summary>
    /// Add a new value to cache using the specified key.  If the key exists, the value will be overwritten with the new value.
    /// </summary>
    /// <param name="key">Key used to identify the cache value.</param>
    /// <param name="value">Value to store in cache.</param>
    /// <param name="expiration">Cache value expiration overriding the configured default expiration.</param>
    void AddCacheValue(string key, object value, TimeSpan expiration);

    /// <summary>
    /// Add a new value to cache using the specified key.  If the key exists, the value will be overwritten with the new value.
    /// </summary>
    /// <param name="key">Key used to identify the cache value.</param>
    /// <param name="value">Value to store in cache.</param>
    /// <param name="expiration">Cache value expiration overriding the configured default expiration.</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Task adding a value to cache</returns>
    Task AddCacheValueAsync(string key, object value, TimeSpan expiration, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a value from cache using the specified key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">Key used to identify the cache value.</param>
    /// <returns>Instance of specified type if it exists in cache, else returns null.</returns>
    T? GetCacheValue<T>(string key);

    /// <summary>
    /// Retrieves a value from cache using the specified key
    /// </summary>
    /// <typeparam name="T">Object type instance of the cached value.</typeparam>
    /// <param name="key">Key used to identify the cache value.</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Task that returns a value from cache if exists and null if not.</returns>
    Task<T?> GetCacheValueAsync<T>(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the cached value for the specified key.
    /// </summary>
    /// <param name="key">Key used to identify the cache value.</param>
    void RemoveCacheValue(string key);

    /// <summary>
    /// Removes the cached value for the specified key.
    /// </summary>
    /// <param name="key">Key used to identify the cache value.</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Task removing a value from cache</returns>
    Task RemoveCacheValueAsync(string key, CancellationToken cancellationToken);
}
