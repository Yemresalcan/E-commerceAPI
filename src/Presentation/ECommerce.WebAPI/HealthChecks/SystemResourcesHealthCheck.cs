using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace ECommerce.WebAPI.HealthChecks;

/// <summary>
/// Health check for system resources (memory, CPU, disk space)
/// </summary>
public class SystemResourcesHealthCheck(ILogger<SystemResourcesHealthCheck> logger) : IHealthCheck
{
    private const long MemoryThresholdBytes = 1024 * 1024 * 1024; // 1 GB
    private const double CpuThresholdPercent = 80.0; // 80%
    private const long DiskSpaceThresholdBytes = 1024 * 1024 * 1024; // 1 GB

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Checking system resources health");

            var healthData = new Dictionary<string, object>();
            var issues = new List<string>();

            // Check memory usage
            var process = Process.GetCurrentProcess();
            var memoryUsage = process.WorkingSet64;
            var availableMemory = GC.GetTotalMemory(false);
            
            healthData["memory_usage_bytes"] = memoryUsage;
            healthData["memory_usage_mb"] = memoryUsage / (1024 * 1024);
            healthData["gc_memory_bytes"] = availableMemory;
            healthData["gc_memory_mb"] = availableMemory / (1024 * 1024);

            if (memoryUsage > MemoryThresholdBytes)
            {
                issues.Add($"High memory usage: {memoryUsage / (1024 * 1024)} MB");
            }

            // Check CPU usage (simplified check using process CPU time)
            var cpuUsage = 0.0;
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    cpuCounter.NextValue(); // First call returns 0
                    await Task.Delay(100, cancellationToken); // Wait a bit for accurate reading
                    cpuUsage = cpuCounter.NextValue();
                }
                else
                {
                    // For non-Windows platforms, use a simplified approach
                    cpuUsage = process.TotalProcessorTime.TotalMilliseconds / Environment.TickCount * 100;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not retrieve CPU usage");
                cpuUsage = 0;
            }
            
            healthData["cpu_usage_percent"] = Math.Round(cpuUsage, 2);

            if (cpuUsage > CpuThresholdPercent)
            {
                issues.Add($"High CPU usage: {cpuUsage:F2}%");
            }

            // Check disk space (for the current drive)
            var currentDirectory = Directory.GetCurrentDirectory();
            var driveInfo = new DriveInfo(Path.GetPathRoot(currentDirectory) ?? "C:");
            
            if (driveInfo.IsReady)
            {
                var freeSpace = driveInfo.AvailableFreeSpace;
                var totalSpace = driveInfo.TotalSize;
                var usedSpace = totalSpace - freeSpace;
                var usedPercentage = (double)usedSpace / totalSpace * 100;

                healthData["disk_free_bytes"] = freeSpace;
                healthData["disk_free_gb"] = freeSpace / (1024 * 1024 * 1024);
                healthData["disk_total_bytes"] = totalSpace;
                healthData["disk_total_gb"] = totalSpace / (1024 * 1024 * 1024);
                healthData["disk_used_percent"] = Math.Round(usedPercentage, 2);

                if (freeSpace < DiskSpaceThresholdBytes)
                {
                    issues.Add($"Low disk space: {freeSpace / (1024 * 1024 * 1024)} GB free");
                }
            }

            // Check thread pool status
            ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out int availableCompletionPortThreads);
            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);

            healthData["available_worker_threads"] = availableWorkerThreads;
            healthData["max_worker_threads"] = maxWorkerThreads;
            healthData["available_completion_port_threads"] = availableCompletionPortThreads;
            healthData["max_completion_port_threads"] = maxCompletionPortThreads;

            var workerThreadUsage = (double)(maxWorkerThreads - availableWorkerThreads) / maxWorkerThreads * 100;
            healthData["worker_thread_usage_percent"] = Math.Round(workerThreadUsage, 2);

            if (workerThreadUsage > 80)
            {
                issues.Add($"High thread pool usage: {workerThreadUsage:F2}%");
            }

            // Add general system information
            healthData["processor_count"] = Environment.ProcessorCount;
            healthData["machine_name"] = Environment.MachineName;
            healthData["os_version"] = Environment.OSVersion.ToString();
            healthData["framework_version"] = Environment.Version.ToString();
            healthData["uptime_seconds"] = (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalSeconds;

            if (issues.Count == 0)
            {
                return HealthCheckResult.Healthy("System resources are within normal limits", healthData);
            }

            if (issues.Count <= 2)
            {
                return HealthCheckResult.Degraded(
                    $"Some system resources are under pressure: {string.Join(", ", issues)}", 
                    null, 
                    healthData);
            }

            return HealthCheckResult.Unhealthy(
                $"Multiple system resources are under pressure: {string.Join(", ", issues)}", 
                null, 
                healthData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking system resources health");
            return HealthCheckResult.Unhealthy("Error checking system resources health", ex);
        }
    }
}