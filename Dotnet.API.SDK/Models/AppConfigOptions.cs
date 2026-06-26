namespace Dotnet.API.SDK.Models;

public record AppConfigOptions
{
    public const string ConfigKey = "AppConfiguration";

    public string? ConnectionString { get; set; }
    public string? Endpoint { get; set; }
}
