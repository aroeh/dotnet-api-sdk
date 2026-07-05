namespace Dotnet.API.SDK.Models;

/// <summary>
/// Business Object definition for Pagination Query parameters.
/// </summary>
/// <remarks>
/// Best used for orchestration, business, or data layers
/// </remarks>
/// <param name="Page">Page number to retrieve.  Defaults to 1</param>
/// <param name="PageSize">Number of results to return for each page.  Defaults to 25</param>
public record PaginationQueryParametersBO
(
    int Page,
    int PageSize
)
{
    public PaginationQueryParametersBO()
        : this(1, 25) { }
}