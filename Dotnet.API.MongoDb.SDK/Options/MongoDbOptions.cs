namespace Dotnet.API.MongoDb.SDK.Options;

/// <summary>
/// Options configuration for MongoDb
/// </summary>
public record MongoDbOptions
{
    /// <summary>
    /// Default Key for configuration values
    /// </summary>
    public const string ConfigKey = "Api:Settings:MongoDb";

    /// <summary>
    /// Connection string for the MongoDb cluster
    /// </summary>
    public string? ConnectionString { get; set; }
    
    /// <summary>
    /// Name of the database to use in the connection
    /// </summary>
    public string? DatabaseName { get; set; }
}
