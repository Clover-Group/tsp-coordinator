using System.Collections.ObjectModel;

namespace TspCoordinator.Data;

public class JobLifecycle
{
    private Dictionary<DateTime, String> events = new Dictionary<DateTime, String>();

    public ReadOnlyDictionary<DateTime, String> Events { get => new ReadOnlyDictionary<DateTime, string>(events); }

    private List<(DateTime, DateTime?)> runningTimes = new List<(DateTime, DateTime?)>();

    private readonly Job job;

    public String JobId => job.JobId;

    public JobLifecycle(Job job)
    {
        this.job = job;
    }

    public void AddQueued()
    {
        events.Add(DateTime.Now, $"Job {JobId} was queued.");
    }

    public void AddLogMessage(String message)
    {
        events.Add(DateTime.Now, $"Job {JobId} logged message: {message}");
    }

    public void AddStatusChanged(JobStatus newStatus)
    {
        events.Add(DateTime.Now, $"Status of {JobId} was changed to {newStatus}");

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
        events.Add(DateTime.Now, $"Job {JobId} {status}.");
    }

    public void AddExternalDiscovered()
    {
        events.Add(
            DateTime.Now,
            $"Job {JobId} was discovered as external on a TSP instance. Not all events may be recorded"
            );
    }
}