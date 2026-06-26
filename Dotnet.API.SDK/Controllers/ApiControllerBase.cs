using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dotnet.API.SDK.Controllers;

[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class ApiControllerBase<TDerived>
(
    ILogger<TDerived> logger
) : ControllerBase
{
    protected ILogger<TDerived> Logger { get; init; } = logger;
}