using Dotnet.API.SDK.Models;

namespace Demo.API.Shared.Models;

public record RestuarantFilterQueryParametersBO
(
    string[]? Names,
    string? CuisineType
) : PaginationQueryParametersBO;
