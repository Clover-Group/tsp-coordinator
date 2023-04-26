using System.Collections.ObjectModel;

namespace TspCoordinator.Data;

public class JobLifecycle
{
    private Dictionary<DateTime, String> events = new Dictionary<DateTime, String>();

    public ReadOnlyDictionary<DateTime, String> Events { get => new ReadOnlyDictionary<DateTime, string>(events); }

    private List<(DateTime, DateTime?)> runningTimes = new List<(DateTime, DateTime?)>();

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

        if (newStatus == JobStatus.Running)
        {
            runningTimes.Add((DateTime.Now, null));
        }
        else if (runningTimes.Count > 0 && runningTimes.LastOrDefault().Item2 is null)
        {
            runningTimes[^1] = (runningTimes[^1].Item1, DateTime.Now);
        }
    }

    public TimeSpan RunningTime => 
        runningTimes
        .Select(range => (range.Item2 ?? DateTime.Now) - range.Item1)
        .Aggregate(TimeSpan.Zero, (sum, value) => sum + value);

    public void AddFinished(bool success, string? error)
    {
        var status = success ? "finished successfully" : $"failed with error: {error ?? "no error text reported"}";
        events.Add(DateTime.Now, $"Job {jobId} {status}.");
    }

    public void AddExternalDiscovered()
    {
        events.Add(
            DateTime.Now,
            $"Job {jobId} was discovered as external on a TSP instance. Not all events may be recorded"
            );
    }
}