using System.Collections.ObjectModel;

namespace TspCoordinator.Data;

public class JobLifecycle
{
    private Dictionary<DateTime, String> events = new Dictionary<DateTime, String>();

    public ReadOnlyDictionary<DateTime, String> Events { get => new ReadOnlyDictionary<DateTime, string>(events); }

    private readonly string jobId;

    public JobLifecycle(Job job)
    {
        jobId = job.JobId;
    }

    public void AddQueued()
    {
        events.Add(DateTime.Now, $"Job {jobId} was queued.");
    }

    public void AddStatusChanged(JobStatus newStatus)
    {
        events.Add(DateTime.Now, $"Status of {jobId} was changed to {newStatus}");
    }

    public void AddFinished(bool success)
    {
        events.Add(DateTime.Now, $"Job {jobId} {(success ? "finished successfully" : "failed")}.");
    }
}