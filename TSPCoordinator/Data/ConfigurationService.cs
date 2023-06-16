namespace TspCoordinator.Data;

public class StatusReportingSettings
{
    public string Broker { get; set; } = default! ;
    public string Topic { get; set; } = default! ;
}

public class QueueStorageRedisSettings
{
    public string Host { get; set; } = default! ;
}

public class ConfigurationService
{
    private ILogger<ConfigurationService> _logger;

    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger;

        var jobReportingLogInvalidEnabledValue = (string? x) =>
        {
            var setTo = x != null ? $"set to {x}" : "not set";
            _logger.LogWarning($"JOB_REPORTING_ENABLED {setTo}, which is not a valid value. Defaulting to false");
            return false;
        };
        var jobReportingEnabled = Environment.GetEnvironmentVariable("JOB_REPORTING_ENABLED") switch
        {
            "true" or "on" or "yes" or "1" => true,
            "false" or "off" or "no" or "0" => false,
            string x => jobReportingLogInvalidEnabledValue(x),
            _ => jobReportingLogInvalidEnabledValue(null)
        };
        if (jobReportingEnabled)
        {
            var broker = Environment.GetEnvironmentVariable("JOB_REPORTING_BROKER");
            var topic = Environment.GetEnvironmentVariable("JOB_REPORTING_TOPIC");
            if (broker == null || topic == null)
            {
                _logger.LogWarning("Job reporting enabled, but broker and/or topic not set, disabling job reporting");
            }
            else
            {
                StatusReportingSettings = new StatusReportingSettings
                {
                    Broker = broker,
                    Topic = topic
                };
            }
        }
        var queueStorageLogInvalidEnabledValue = (string? x) =>
        {
            var setTo = x != null ? $"set to {x}" : "not set";
            _logger.LogWarning($"QUEUE_STORAGE_ENABLED {setTo}, which is not a valid value. Defaulting to false");
            return false;
        };
        var queueStorageEnabled = Environment.GetEnvironmentVariable("QUEUE_STORAGE_ENABLED") switch
        {
            "true" or "on" or "yes" or "1" => true,
            "false" or "off" or "no" or "0" => false,
            string x => queueStorageLogInvalidEnabledValue(x),
            _ => queueStorageLogInvalidEnabledValue(null)
        };
        if (queueStorageEnabled)
        {
            var host = Environment.GetEnvironmentVariable("QUEUE_STORAGE_HOST");
            if (host == null)
            {
                _logger.LogWarning("Queue storage enabled, but host not set, disabling queue storage");
            }
            else
            {
                QueueStorageRedisSettings = new QueueStorageRedisSettings
                {
                    Host = host
                };
            }
        }


        var queueInspectionIntervalVar = Environment.GetEnvironmentVariable("QUEUE_INSPECTION_INTERVAL") ?? "";
        if (UInt64.TryParse(queueInspectionIntervalVar, out var queueInspectionInterval))
        {
            QueueInspectionInterval = queueInspectionInterval;
        }
        else
        {
            _logger.LogWarning($"Queue inspection interval not set, defaulting to {QueueInspectionInterval}");
        }
        var healthCheckIntervalVar = Environment.GetEnvironmentVariable("HEALTH_CHECK_INTERVAL") ?? "";
        if (UInt64.TryParse(healthCheckIntervalVar, out var healthCheckInterval))
        {
            HealthCheckInterval = healthCheckInterval;
        }
        else
        {
            _logger.LogWarning($"Queue inspection interval not set, defaulting to {HealthCheckInterval}");
        }
        var cleanupCompletedIntervalVar = Environment.GetEnvironmentVariable("CLEANUP_COMPLETED_INTERVAL") ?? "";
        if (UInt64.TryParse(cleanupCompletedIntervalVar, out var cleanupCompletedInterval))
        {
            CleanupCompletedInterval = cleanupCompletedInterval;
        }
        else
        {
            _logger.LogWarning($"Completed jobs cleanup interval not set, defaulting to {CleanupCompletedInterval}");
        }
        var maxJobsPerTspVar = Environment.GetEnvironmentVariable("MAX_JOBS_PER_TSP") ?? "";
        if (UInt32.TryParse(maxJobsPerTspVar, out var maxJobsPerTsp))
        {
            MaxJobsPerTsp = maxJobsPerTsp;
        }
        else
        {
            _logger.LogWarning($"Max jobs per TSP instance not set, defaulting to {MaxJobsPerTsp}");
        }
        var healthCheckAttemptsVar = Environment.GetEnvironmentVariable("HEALTH_CHECK_ATTEMPTS") ?? "";
        if (UInt32.TryParse(healthCheckAttemptsVar, out var healthCheckAttempts))
        {
            HealthCheckAttempts = healthCheckAttempts;
        }
        else
        {
            _logger.LogWarning($"Health check attempts not set, defaulting to {HealthCheckAttempts}");
        }
    }

    public StatusReportingSettings? StatusReportingSettings { get; private set; }

    public QueueStorageRedisSettings? QueueStorageRedisSettings { get; private set; }

    public ulong QueueInspectionInterval { get; private set; } = 5000;

    public ulong HealthCheckInterval { get; private set; } = 10000;

    public ulong CleanupCompletedInterval { get; private set; } = 1_800_000;

    public uint MaxJobsPerTsp { get; set; } = 1;

    public uint HealthCheckAttempts { get; set; } = 10;

}