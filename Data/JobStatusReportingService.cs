using System;
using System.Globalization;
using System.Text.Json;
using Confluent.Kafka;
using Dahomey.Json;

namespace TspCoordinator.Data;

public class StatusMessage
{
   public string Uuid { get; set; }
   public string Timestamp { get; set; }
   public string Status { get; set; }
   public string Text { get; set; }
}

public class JobStatusReportingService
{
    private readonly bool jobReportingEnabled;
    private readonly string? jobReportingBroker;
    private readonly string? jobReportingTopic;

    private readonly ProducerConfig? producerConfig;
    private readonly IProducer<String, String>? producer;

    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    }.SetupExtensions();


    private ILogger<JobStatusReportingService> _logger;

    public JobStatusReportingService(ILogger<JobStatusReportingService> logger)
    {
        _logger = logger;

        var logInvalidEnabledValue = (string? x) => {
            var setTo = x != null ? $"set to {x}" : "not set";
            _logger.LogWarning($"JOB REPORTING_ENABLED {setTo}, which is not a valid value. Defaulting to false");
            return false;
        };

        jobReportingEnabled = Environment.GetEnvironmentVariable("JOB_REPORTING_ENABLED") switch
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
                jobReportingEnabled = false;
            }
            else
            {
                jobReportingBroker = broker;
                jobReportingTopic = topic;
                producerConfig = new ProducerConfig
                {
                    BootstrapServers = broker,
                    ClientId = "TspCoordinatorStatusReporter"
                };
                producer = new ProducerBuilder<string, string>(producerConfig).Build();
            }
        }
    }

    public void SendJobStatus(Job job, string text)
    {
        if (jobReportingEnabled)
        {
            var now = DateTime.Now.ToString("u", CultureInfo.InvariantCulture);
            var message = new StatusMessage
            {
                Uuid = Guid.NewGuid().ToString(),
                Timestamp = now,
                Status = job.Status.ToString(),
                Text = text
            };
            var messageAsJson = JsonSerializer.Serialize(message, jsonOptions);
            producer?.Produce(jobReportingTopic, new Message<string, string> { Key = now, Value = messageAsJson });
        }
    }
}