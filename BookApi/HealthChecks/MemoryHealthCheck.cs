using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace BookApi.HealthChecks;

public class MemoryHealthCheck : IHealthCheck
{
	private readonly long _thresholdInBytes;

	public MemoryHealthCheck(long thresholdInBytes = 1_000_000_000) // 1GB
	{
		_thresholdInBytes = thresholdInBytes;
	}

	public Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		var allocatedBytes = GC.GetTotalMemory(forceFullCollection: false);
		var data = new Dictionary<string, object?>
		{
			["AllocatedBytes"] = allocatedBytes,
			["ThresholdBytes"] = _thresholdInBytes
		};

		var status = allocatedBytes < _thresholdInBytes
			? HealthStatus.Healthy
			: HealthStatus.Degraded;

		return Task.FromResult(new HealthCheckResult(
			status,
			description: $"Memory: {allocatedBytes / 1_000_000} MB",
			data: data));
	}
}
