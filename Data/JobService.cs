namespace TSPCoordinator.Data;
using System.Collections.Generic;
using TSPCoordinator.Controllers;

public class PriorityComparer : IComparer<int>
{
    public int Compare(int x, int y) => -x.CompareTo(y);
}

public class JobService
{
    private PriorityQueue<Job, int> jobQueue = new PriorityQueue<Job, int>(new PriorityComparer());

    private Job[] runningJobs =
    {
        new Job
        {
            JobId = "test2-01",
            Status = JobStatus.Running,
            RowsRead = 3827,
            RowsWritten = 114,
            Priority = 65
        },
        new Job
        {
            JobId = "test2-02",
            Status = JobStatus.Running,
            RowsRead = 61402,
            RowsWritten = 816,
            Priority = 85
        },
    };


    private Job[] completedJobs =
    {
        new Job
        {
            JobId = "test3-01",
            Status = JobStatus.Finished,
            RowsRead = 118213,
            RowsWritten = 1602,
            Priority = 80
        },
        new Job
        {
            JobId = "test3-02",
            Status = JobStatus.Failed,
            RowsRead = 1751,
            RowsWritten = 0,
            Priority = 70
        },
        new Job
        {
            JobId = "test3-03",
            Status = JobStatus.Canceled,
            RowsRead = 909,
            RowsWritten = 3,
            Priority = 30
        }
    };

    public JobService()
    {
        jobQueue.Enqueue(new Job { JobId = "test1-01", Status = JobStatus.Enqueued, Priority = 80 }, 80);
        jobQueue.Enqueue(new Job { JobId = "test1-02", Status = JobStatus.Enqueued, Priority = 90 }, 90);
    }

    public Task<Job[]> GetJobQueueAsync()
    {
        return Task.FromResult(jobQueue.UnorderedItems.OrderByDescending(x => x.Priority).Select(x => x.Element).ToArray());
    }

    public Task<Job[]> GetRunningJobsAsync()
    {
        return Task.FromResult(runningJobs);
    }

    public Task<Job[]> GetCompletedQueueAsync()
    {
        return Task.FromResult(completedJobs);
    }

    public void OnJobStarted(JobStartedInfo info)
    {
        // TODO: Job started
    }

    public void OnJobCompleted(JobCompletedInfo info)
    {
        // TODO: Job completed
    }
}