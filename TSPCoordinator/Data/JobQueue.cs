namespace TspCoordinator.Data;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Dahomey.Json;
using StackExchange.Redis;

public class JobQueue
{
    private List<Job> jobs = new List<Job>();

    private readonly ConnectionMultiplexer? _redis;

    private string _storageKey;

    private JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };


    public JobQueue(string? redisHost, string storageKey)
    {
        _storageKey = storageKey;
        foreach (var c in TspCoordinator.Data.TspApi.JsonConverters.Converters)
        {
            jsonOptions.Converters.Add(c);
        }
        jsonOptions.SetupExtensions();


        if (redisHost != null)
        {
            _redis = ConnectionMultiplexer.Connect(redisHost);
        }

        if (_redis != null)
        {
            var db = _redis.GetDatabase();
            var serializedQueue = (string?)db.StringGet(_storageKey);
            try
            {
                jobs = (JsonSerializer.Deserialize(serializedQueue ?? "[]", typeof(List<Job>), jsonOptions) as List<Job>)
                    ?? new List<Job>();
                Console.WriteLine($"Queue restored: {jobs.Count} jobs recovered");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not serialize storage queue: {ex.Message}");
            }
        }
    }

    public ReadOnlyCollection<Job> Jobs { get => jobs.AsReadOnly(); }

    public void Enqueue(Job job)
    {
        try
        {
            int index = 0;
            while (index < jobs.Count && jobs[index]?.Request is not null && (jobs[index].Request.Priority > job.Request.Priority
                   || (jobs[index].Request.Priority == job.Request.Priority && jobs[index].ArrivalTime < job.ArrivalTime)))
            {
                if (jobs[index] is null) Console.WriteLine($"Null occured in the job queue at position {index}");
                else if (jobs[index].Request is null) Console.WriteLine($"Request for job {job.JobId} is null");
                index++;
            }
            jobs.Insert(index, job);
            Persist();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not enqueue the job: {ex.Message}");
            if (job is not null)
            {
                string json = JsonSerializer.Serialize<Job>(job);
                Console.WriteLine($"Job data: {json}");
            }
            else
            {
                Console.WriteLine("Job is null for some unknown reason");
            }
        }
    }

    // Enqueue the job to the end of the queue, ignoring priority
    public void EnqueueToEnd(Job job)
    {
        jobs.Add(job);
    }


    public Job? Dequeue()
    {
        if (jobs.Count > 0)
        {
            var res = jobs[0];
            jobs.RemoveAt(0);
            Persist();
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
                Persist();
                return true;
        }
    }

    public int RemoveNulls() => jobs.RemoveAll(j => j is null);

    private void Persist()
    {
        //Console.WriteLine("Persisting...");
        if (_redis != null)
        {
            var jobsCopy = jobs;
            var serialized = JsonSerializer.Serialize(jobsCopy, typeof(List<Job>), jsonOptions);
            var db = _redis.GetDatabase();
            db.StringSet(_storageKey, serialized);
        }
    }

    public Dictionary<JobStatus, int> GetCountsByStatus() =>
        jobs.GroupBy(job => job.Status).ToDictionary(group => group.Key, group => group.Count());
}
