namespace TspCoordinator.Data;

using System.Text.Json.Serialization;
using Actual = TspApi.V3;

public enum JobStatus {
    Enqueued,
    Running,
    Failed,
    Finished,
    Canceled
}

public class JobStatusInfo {
    public long RowsRead { get; set; }
    public long RowsWritten { get; set; }
}

public class Job {
    public Job() => Lifecycle = new JobLifecycle(this);

    public string JobId { get; set; }
    public JobStatus Status { get; set; }

    public long RowsRead { get; set; }
    public long RowsWritten { get; set; }

    public Actual.Request Request { get; set; }

    public TspInstance? RunningOn { get; set; }

    [JsonIgnore]
    public JobLifecycle Lifecycle { get; private set; }

    public void NotifyStatusChanged() => Lifecycle.AddStatusChanged(Status);
}