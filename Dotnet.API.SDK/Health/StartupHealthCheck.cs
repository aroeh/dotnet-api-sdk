using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dotnet.API.SDK.Health;

public class StartupHealthCheck : IHealthCheck
{
    private volatile bool _isReady;
    public bool StartupCompleted
    {
        get => _isReady;
        set => _isReady = value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = new())
    {
        try
        {
            if (StartupCompleted)
            {
                return HealthCheckResult.Healthy("The API startup has completed.");
            }

            return HealthCheckResult.Unhealthy("That API startup is still running.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Unable to determine API startup status", ex);
        }
    }
}
