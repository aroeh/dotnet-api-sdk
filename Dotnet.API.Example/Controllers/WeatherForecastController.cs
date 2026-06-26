using Asp.Versioning;
using Dotnet.API.SDK.Controllers;
using Dotnet.API.SDK.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet.API.Example.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("[controller]")]
public class WeatherForecastController
(
    ILogger<WeatherForecastController> logger
): ApiControllerBase<WeatherForecastController>(logger)
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet("{id}")]
    public IEnumerable<WeatherForecast> Get([FromRoute] string id)
    {
        Logger.LogInformation("Demo for logging");
        throw new NotFoundException("This is a demo for exceptions", id);
    }
}
