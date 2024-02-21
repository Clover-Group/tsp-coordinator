using System;
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

public enum TspCapability
{
    CSVSparseIntermediate
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

    public Version Version { get; set; } = new Version(0, 0, 0);

    public TspInstanceStatus Status { get; set; }

    public DateTime HealthCheckDate { get; set; }

    public uint HealthCheckAttemptsRemaining { get; set; }

    public List<String> RunningJobsIds { get; set; } = new List<string>();

    public List<String> SentJobsIds { get; set; } = new List<string>();

    public int RunningJobsCount => RunningJobsIds?.Count ?? 0;

    public int SentJobsCount => SentJobsIds?.Count ?? 0;

    public int TotalJobCount => RunningJobsCount + SentJobsCount;

    public bool SupportsCapability(TspCapability capability) => capability switch
    {
        TspCapability.CSVSparseIntermediate => Version >= new Version(19, 6, 0),
        _ => false,
    };

    public override string ToString()
    {
        return $"TSP instance {Uuid} v{Version} at {Location}";
    }
}
