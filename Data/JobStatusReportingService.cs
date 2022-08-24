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
    private readonly StatusReportingSettings? statusReportingSettings;

    private readonly ProducerConfig? producerConfig;
    private readonly IProducer<String, String>? producer;

    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    }.SetupExtensions();


    private ILogger<JobStatusReportingService> _logger;

    private ConfigurationService _configurationService;

    public JobStatusReportingService(ILogger<JobStatusReportingService> logger, ConfigurationService configurationService)
    {
        _logger = logger;
        _configurationService = configurationService;
        statusReportingSettings = _configurationService.StatusReportingSettings;
        if (statusReportingSettings != null)
        {
            producerConfig = new ProducerConfig
            {
                BootstrapServers = statusReportingSettings.Broker,
                ClientId = "TspCoordinatorStatusReporter"
            };
            producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }
    }


    public void SendJobStatus(Job job, string text)
    {
        if (statusReportingSettings != null)
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
            producer?.Produce(statusReportingSettings.Topic, new Message<string, string> { Key = now, Value = messageAsJson });
        }
    }
}