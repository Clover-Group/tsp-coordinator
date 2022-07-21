using System.Net;
using System.Text.Json.Serialization;

namespace TspCoordinator.Data;

public enum TspInstanceStatus {
    Active,
    NotWorking,
    NotResponding
}

public class TspInstance
{

    [JsonIgnore]
    public IPAddress Host { get; set; }

    public int Port { get; set; }

    [JsonInclude]
    public string Location => $"{Host.MapToIPv4()}:{Port}";

    public string? Version { get; set; }

    public TspInstanceStatus Status { get; set; }

    public DateTime HealthCheckDate { get; set; }
}
