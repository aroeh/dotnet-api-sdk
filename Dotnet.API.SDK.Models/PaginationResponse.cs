namespace Dotnet.API.SDK.Models;

public record PaginationResponse<T>
(
    IEnumerable<T> Data,
    PaginationMetaData MetaData
) where T : class
{
    // adding parameterless default constructor for deserialization
    public PaginationResponse() : this([], new PaginationMetaData())
    {

    }
}
