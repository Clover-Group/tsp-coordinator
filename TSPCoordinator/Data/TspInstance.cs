using System.Net;
using System.Text.Json.Serialization;

namespace TspCoordinator.Data;

public enum TspInstanceStatus
{
    Active,
    NotWorking,
    NotResponding,
    Busy,
    CannotGetExtendedInfo
}

public class TspInstance
{
    public Guid Uuid { get; set; }

    [JsonIgnore]
    public IPAddress Host { get; set; } = default! ;

    [JsonIgnore]
    public int Port { get; set; }

    [JsonInclude]
    public string Location => $"{Host.MapToIPv4()}:{Port}";

    [JsonIgnore]
    public bool IsHostAdvertised { get; set; } = false;

    [JsonIgnore]
    public bool IsPortAdvertised { get; set; } = false;

    public string? Version { get; set; }

    public TspInstanceStatus Status { get; set; }

    public DateTime HealthCheckDate { get; set; }

    public uint HealthCheckAttemptsRemaining { get; set; }

    public List<String> RunningJobsIds { get; set; } = new List<string>();

    public List<String> SentJobsIds { get; set; } = new List<string>();

    public int RunningJobsCount => RunningJobsIds?.Count ?? 0;

    public int SentJobsCount => SentJobsIds?.Count ?? 0;

    public int TotalJobCount => RunningJobsCount + SentJobsCount;

    public override string ToString()
    {
        return $"TSP instance {Uuid} v{Version} at {Location}";
    }
}
