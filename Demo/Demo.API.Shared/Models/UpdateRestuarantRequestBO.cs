namespace Demo.API.Shared.Models;

public record UpdateRestuarantRequestBO
(
    string? Name,
    string? CuisineType,
    Uri? Website,
    string? Phone,
    UpdateLocationRequestBO? Address
);
