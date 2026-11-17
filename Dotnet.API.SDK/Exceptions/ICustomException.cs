namespace Dotnet.API.SDK.Exceptions;

public interface ICustomException
{
    /// <summary>
    /// Status Code to use for the exception response and problem details
    /// </summary>
    int StatusCode { get; }

    /// <summary>
    /// Title text to display in the problem details response
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Key value used to retrieve data from the problem details data extension
    /// </summary>
    string DataKey { get; }
}
