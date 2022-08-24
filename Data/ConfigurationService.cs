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
        var healthCheckIntervalVar = Environment.GetEnvironmentVariable("HEALTH_CHECK_INTERVAL") ?? "";
        ulong queueInspectionInterval, healthCheckInterval;
        if (UInt64.TryParse(queueInspectionIntervalVar, out queueInspectionInterval))
        {
            QueueInspectionInterval = queueInspectionInterval;
        }
        else
        {
            _logger.LogWarning($"Queue inspection interval not set, defaulting to {QueueInspectionInterval}");
        }
        if (UInt64.TryParse(healthCheckIntervalVar, out healthCheckInterval))
        {
            HealthCheckInterval = healthCheckInterval;
        }
        else
        {
            _logger.LogWarning($"Queue inspection interval not set, defaulting to {HealthCheckInterval}");
        }
    }

    public StatusReportingSettings? StatusReportingSettings { get; private set; }

    public ulong QueueInspectionInterval { get; private set; } = 5000;

    public ulong HealthCheckInterval { get; private set; } = 10000;
    
}