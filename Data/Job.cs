namespace TspCoordinator.Data;

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

    public long Priority { get; set; }
}