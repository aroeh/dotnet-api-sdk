namespace Dotnet.API.MSSQL.SDK.Options;

/// <summary>
/// Options configuration for MS SQL
/// </summary>
public record MSSqlOptions
{
    /// <summary>
    /// Default Key for configuration values
    /// </summary>
    public const string ConfigKey = "Api:Settings:MSSql";

    /// <summary>
    /// Connection string for the MS SQL database
    /// </summary>
    public string? ConnectionString { get; set; }
}
