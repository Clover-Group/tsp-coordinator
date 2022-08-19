namespace TspCoordinator.Data;

using System.Collections.Generic;
using System.Collections.ObjectModel;

public class JobQueue
{
    private List<Job> jobs = new List<Job>();

    public ReadOnlyCollection<Job> Jobs { get => jobs.AsReadOnly(); }

    public void Enqueue(Job job)
    {
        int index = 0;
        while (index < jobs.Count && jobs[index].Request.Priority > job.Request.Priority)
        {
            index++;
        }
        jobs.Insert(index, job);
    } 
    

    public Job? Dequeue()
    {
        if(jobs.Count > 0)
        {
            var res = jobs[0];
            jobs.RemoveAt(0);
            return res;
        }
        return null;
    }

    public Job? FindById(string jobId) => jobs.Find(j => j.JobId == jobId);

    public bool RemoveById(string jobId) 
    {
        switch (FindById(jobId))
        {
            case null:
                return false;
            case Job job:
                jobs.Remove(job);
                return true;
        }
    }
}