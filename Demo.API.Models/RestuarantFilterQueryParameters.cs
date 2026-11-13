using Dotnet.API.SDK.Models;
using Microsoft.AspNetCore.Mvc;

namespace Demo.API.Models;

public record RestuarantFilterQueryParameters : PaginationQueryParameters
{
    [FromQuery(Name = "name")]
    public string[]? Names { get; init; } = default!;

    [FromQuery(Name = "cuisine")]
    public string? CuisineType { get; init; } = default!;
}
