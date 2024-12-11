using System;
using System.Net;
using System.Text.Json.Serialization;
using Semver;

namespace TspCoordinator.Data;

public enum TspInstanceStatus
{
    Active,
    RestartScheduled,
    NotWorking,
    NotResponding,
    Busy,
    CannotGetExtendedInfo
}

public enum TspCapability
{
    CSVSparseIntermediate,
    TotalJobsLimit
}

public class TspInstance
{
    public Guid Uuid { get; set; }

    [JsonIgnore]
    public IPAddress Host { get; set; } = default!;

    [JsonIgnore]
    public int Port { get; set; }

    [JsonInclude]
    public string Location => $"{Host.MapToIPv4()}:{Port}";

    [JsonIgnore]
    public bool IsHostAdvertised { get; set; } = false;

    [JsonIgnore]
    public bool IsPortAdvertised { get; set; } = false;

    public SemVersion Version { get; set; } = new SemVersion(0, 0, 0);

    public TspInstanceStatus Status { get; set; }

    public DateTime HealthCheckDate { get; set; }

    public uint HealthCheckAttemptsRemaining { get; set; }

    public List<String> RunningJobsIds { get; set; } = new List<string>();

    public List<String> SentJobsIds { get; set; } = new List<string>();

    public int RunningJobsCount => RunningJobsIds?.Count ?? 0;

    public int SentJobsCount => SentJobsIds?.Count ?? 0;

    public int TotalJobCount => RunningJobsCount + SentJobsCount;

    public int TotalSentJobsCounter { get; set; } = 0;
    public int TotalFinishedJobsCounter { get; set; } = 0;

    public int TotalJobsLimit { get; set; } = 0;

    public bool SupportsCapability(TspCapability capability) => capability switch
    {
        TspCapability.CSVSparseIntermediate => Version.ComparePrecedenceTo(new SemVersion(19, 6, 0)) > 0,
        TspCapability.TotalJobsLimit => Version.ComparePrecedenceTo(new SemVersion(19, 11, 0)) > 0,
        _ => false,
    };

    public override string ToString()
    {
        return $"TSP instance {Uuid} v{Version} at {Location}";
    }
}
