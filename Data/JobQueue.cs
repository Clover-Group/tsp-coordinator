namespace TspCoordinator.Data;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Dahomey.Json;
using FASTER.core;

public class JobQueue
{
    private List<Job> jobs = new List<Job>();

    private readonly FasterKVSettings<string, string> _settings = 
        new FasterKVSettings<string, string>("/tmp/tsp-coordinator/job-queue") { TryRecoverLatest = true }; 
    private readonly FasterKV<string, string> _store;

    private string _storeName;

    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    }.SetupExtensions();

    public JobQueue()
    {
        foreach (var c in TspCoordinator.Data.TspApi.JsonConverters.Converters)
        {
            jsonOptions.Converters.Add(c);
        }
        _store = new FasterKV<string, string>(_settings);
        using var s = _store.NewSession(new SimpleFunctions<string, string>((a, b) => b));
        _storeName = "job_queue";
        string serialized = "";
        var st = s.Read(ref _storeName, ref serialized);
        Console.WriteLine($"Store name: {_storeName}, status: {st}");
        if (st.Found)
        {
            jobs = (List<Job>)JsonSerializer.Deserialize(serialized, typeof(List<Job>));
        }
    }

    public ReadOnlyCollection<Job> Jobs { get => jobs.AsReadOnly(); }

    public void Enqueue(Job job)
    {
        int index = 0;
        while (index < jobs.Count && jobs[index].Request.Priority > job.Request.Priority)
        {
            index++;
        }
        jobs.Insert(index, job);
        Persist();

    } 
    

    public Job? Dequeue()
    {
        if(jobs.Count > 0)
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

    private void Persist()
    {
        Console.WriteLine("Persisting...");
        using var s = _store.NewSession(new SimpleFunctions<string, string>((a, b) => b));
        var serializedJobs = JsonSerializer.Serialize(jobs);
        var st = s.Upsert(ref _storeName, ref serializedJobs);
        _store.Log.Flush(true);
        Console.WriteLine($"Store name: {_storeName}, status: {st}");
    }
}