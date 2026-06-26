namespace Dotnet.API.SDK.Exceptions;

public interface ICustomException
{
    int StatusCode { get; }
    string Title { get; }
    string DataKey { get; }
}
