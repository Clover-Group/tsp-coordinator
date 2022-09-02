namespace TspCoordinator.Data;

public class StatusReportingSettings
{
    public string Broker { get; set; }
    public string Topic { get; set; }
}

public class ConfigurationService
{
private ILogger<ConfigurationService> _logger;

    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger;

        var logInvalidEnabledValue = (string? x) =>
        {
            var setTo = x != null ? $"set to {x}" : "not set";
            _logger.LogWarning($"JOB REPORTING_ENABLED {setTo}, which is not a valid value. Defaulting to false");
            return false;
        };
        var jobReportingEnabled = Environment.GetEnvironmentVariable("JOB_REPORTING_ENABLED") switch
        {
            "true" or "on" or "yes" or "1" => true,
            "false" or "off" or "no" or "0" => false,
            string x => logInvalidEnabledValue(x),
            _ => logInvalidEnabledValue(null)
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

    public ulong QueueInspectionInterval { get; private set; } = 5000;

    public ulong HealthCheckInterval { get; private set; } = 10000;

    public uint MaxJobsPerTsp { get; set; } = 1;

    public uint HealthCheckAttempts { get; set; } = 10;
    
}