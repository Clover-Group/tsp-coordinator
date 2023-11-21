using System.Collections.ObjectModel;

namespace TspCoordinator.Data;

public abstract record LifecycleEvent
{
    private LifecycleEvent() { }

    public record Enqueued() : LifecycleEvent { }

    public record LoggedMessage(string Message) : LifecycleEvent { }
    
    public record StatusChanged(JobStatus NewStatus) : LifecycleEvent { }

    public record Finished(bool Success, string? Error) : LifecycleEvent { }

    public record ExternalDiscovered() : LifecycleEvent { }
}

public class JobLifecycle
{
    private Dictionary<DateTime, LifecycleEvent> events = new Dictionary<DateTime, LifecycleEvent>();

    public ReadOnlyDictionary<DateTime, LifecycleEvent> Events { get => new ReadOnlyDictionary<DateTime, LifecycleEvent>(events); }

    private List<(DateTime, DateTime?)> runningTimes = new List<(DateTime, DateTime?)>();

    private readonly Job job;

    public String JobId => job.JobId;

    public JobLifecycle(Job job)
    {
        this.job = job;
    }

    public void AddQueued()
    {
        events.Add(DateTime.Now, new LifecycleEvent.Enqueued());
    }

    public void AddLogMessage(String message)
    {
        events.Add(DateTime.Now, new LifecycleEvent.LoggedMessage(message));
    }

    public void AddStatusChanged(JobStatus newStatus)
    {
        events.Add(DateTime.Now, new LifecycleEvent.StatusChanged(newStatus));

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
        //var status = success ? "finished successfully" : $"failed with error: {error ?? "no error text reported"}";
        events.Add(DateTime.Now, new LifecycleEvent.Finished(success, error));
    }

    public void AddExternalDiscovered()
    {
        events.Add(
            DateTime.Now,
            new LifecycleEvent.ExternalDiscovered()
            );
    }
}