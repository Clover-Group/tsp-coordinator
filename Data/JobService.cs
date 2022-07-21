namespace TspCoordinator.Data;
using System.Collections.Generic;
using TspCoordinator.Controllers;

public class PriorityComparer : IComparer<int>
{
    public int Compare(int x, int y) => -x.CompareTo(y);
}

public class JobService
{
    private PriorityQueue<Job, int> jobQueue = new PriorityQueue<Job, int>(new PriorityComparer());

    private List<Job> runningJobs = new List<Job>();


    private List<Job> completedJobs = new List<Job>();

    public Task<List<Job>> GetJobQueueAsync()
    {
        return Task.FromResult(jobQueue.UnorderedItems.OrderByDescending(x => x.Priority).Select(x => x.Element).ToList());
    }

    public Task<List<Job>> GetRunningJobsAsync()
    {
        return Task.FromResult(runningJobs);
    }

    public Task<List<Job>> GetCompletedQueueAsync()
    {
        return Task.FromResult(completedJobs);
    }

    public void OnJobStarted(JobStartedInfo info)
    {
        var job = runningJobs.Find(x => x.JobId == info.JobId);
        if (job == null)
        {
            
        }
        else
        {
            job.Status = JobStatus.Running;
        }
    }

    public void OnJobCompleted(JobCompletedInfo info)
    {
        // TODO: Job completed
    }
}