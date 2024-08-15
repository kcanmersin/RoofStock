using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Quartz;

namespace Core.Health
{

public class QuartzHealthCheck : IHealthCheck
{
    private readonly ISchedulerFactory _schedulerFactory;

    public QuartzHealthCheck(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        if (scheduler != null && scheduler.IsStarted)
        {
            return HealthCheckResult.Healthy("Quartz is running.");
        }

        return HealthCheckResult.Unhealthy("Quartz is not running.");
    }
}
}