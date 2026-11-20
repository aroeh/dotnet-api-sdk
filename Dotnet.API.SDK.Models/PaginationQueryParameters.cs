using Microsoft.AspNetCore.Mvc;

namespace Dotnet.API.SDK.Models;

/// <summary>
/// Pagination query parameter model for API requests
/// </summary>
public record PaginationQueryParameters
{
    /// <summary>
    /// Page number to retrieve
    /// </summary>
    [FromQuery(Name = "page")]
    public int? Page { get; init; } = default!;

    /// <summary>
    /// Number of results to return for each page
    /// </summary>
    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; init; } = default!;
}
