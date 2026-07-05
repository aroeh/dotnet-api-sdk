namespace Dotnet.API.MSSQL.SDK.Entities;

/// <summary>
/// Base entity for Entity Framework core entities
/// </summary>
public class EntityBase
{
    /// <summary>
    /// Id of the entity
    /// </summary>
    public string Id { get; set; } = string.Empty;
}
