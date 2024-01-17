namespace TspCoordinator.Data;

using System.Text.Json.Serialization;
using Actual = TspApi.V3;

public enum JobStatus
{
    Enqueued,
    Running,
    Failed,
    Finished,
    Canceled
}

public class JobStatusInfo
{
    public long RowsRead { get; set; }
    public long RowsWritten { get; set; }
}

public class JobMetricPoint
{
    public DateTime DateTime { get; set; } = DateTime.Now;
    public long RowsRead { get; set; }
    public long RowsWritten { get; set; }

    public static implicit operator JobMetricPoint(JobStatusInfo jobStatusInfo) =>
        new() { RowsRead = jobStatusInfo.RowsRead, RowsWritten = jobStatusInfo.RowsWritten };
}

public class Job
{
    public Job() => Lifecycle = new JobLifecycle(this);

    public string JobId { get; set; } = default!;
    public JobStatus Status { get; set; }

    [JsonIgnore]
    public List<JobMetricPoint> MetricHistory { get; set; } = new();

    [JsonIgnore]
    public List<(long, long)> SpeedData { get; set; } = new();

    public long RowsRead => MetricHistory.LastOrDefault()?.RowsRead ?? 0;
    public long RowsWritten => MetricHistory.LastOrDefault()?.RowsWritten ?? 0;

    public bool IsExternal { get; set; } = false;

    public Actual.Request Request { get; set; } = default!;

    public TspInstance? RunningOn { get; set; }

    [JsonIgnore]
    public JobLifecycle Lifecycle { get; private set; }

    public uint RestartAttempts { get; set; } = 0;

    public void NotifyStatusChanged() => Lifecycle.AddStatusChanged(Status);

    public void CacheSpeed()
    {
        var lastCheckpoints = MetricHistory.TakeLast(10).ToArray();
        var firstOption = lastCheckpoints.FirstOrDefault();
        var lastOption = lastCheckpoints.LastOrDefault();
        var speed = (firstOption, lastOption) switch
        {
            (JobMetricPoint first, JobMetricPoint last) =>
                (
                    (long)Math.Round((last.RowsRead - first.RowsRead) / (last.DateTime - first.DateTime).TotalSeconds),
                    (long)Math.Round((last.RowsWritten - first.RowsWritten) / (last.DateTime - first.DateTime).TotalSeconds)
                ),
            (_, _) => (0, 0)
        };
        if (speed.Item1 < 0) speed.Item1 = 0;
        if (speed.Item2 < 0) speed.Item2 = 0;
        SpeedData.Add(speed);
        if (SpeedData.Count > 10) SpeedData.RemoveAt(0);
    }

    public (long, long) Speed => SpeedData.LastOrDefault();
}