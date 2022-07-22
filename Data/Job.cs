namespace TspCoordinator.Data;

using Actual = TspApi.V3;

public enum JobStatus {
    Enqueued,
    Running,
    Failed,
    Finished,
    Canceled
}

public class Job {
    public string JobId { get; set; }
    public JobStatus Status { get; set; }

    public long RowsRead { get; set; }
    public long RowsWritten { get; set; }

    public Actual.Request Request { get; set; }

    public TspInstance? RunningOn { get; set; }
}