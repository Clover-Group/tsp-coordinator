namespace TspCoordinator.Data;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Dahomey.Json;
using FASTER.core;

public class JobQueue
{
    private List<Job> jobs = new List<Job>();

    private class JobListSerializer : BinaryObjectSerializer<List<Job>>
    {
        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        }.SetupExtensions();

        public JobListSerializer()
        {
            foreach (var c in TspCoordinator.Data.TspApi.JsonConverters.Converters)
            {
                jsonOptions.Converters.Add(c);
            }
        }

        public override void Deserialize(out List<Job> obj)
        {
            var serialized = reader.ReadString();
            obj = (List<Job>)JsonSerializer.Deserialize(serialized, typeof(List<Job>));
        }

        public override void Serialize(ref List<Job> obj)
        {
            var serialized = JsonSerializer.Serialize(obj);
            writer.Write(serialized);
        }
    }

    private readonly FasterKVSettings<string, List<Job>> _settings = 
        new FasterKVSettings<string, List<Job>>("/tmp/tsp-coordinator/job-queue")
        { 
            //TryRecoverLatest = true,
            ValueSerializer = (() => new JobListSerializer())
        }; 
    private readonly FasterKV<string, List<Job>> _store;

    private string _storeName;

    

    public JobQueue()
    {
        _store = new FasterKV<string, List<Job>>(_settings);
        using var s = _store.NewSession(new SimpleFunctions<string, List<Job>>((a, b) => a.Concat(b).Distinct().ToList()));
        _storeName = "job_queue";
        if (_store.RecoveredVersion != 1)
        {
            Console.WriteLine($"Reading store version {_store.RecoveredVersion}...");
            Console.Out.Flush();
            var st = s.Read(ref _storeName, ref jobs);
            Console.WriteLine($"Store name: {_storeName}, status: {st}");
        }
        else
        {
            Console.WriteLine($"Store is version {_store.RecoveredVersion}, starting from scratch");
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
        using var s = _store.NewSession(new SimpleFunctions<string, List<Job>>((a, b) => a.Concat(b).Distinct().ToList()));
        var st = s.Upsert(ref _storeName, ref jobs);
        _store.TryInitiateFullCheckpoint(out _, CheckpointType.Snapshot);
        _store.CompleteCheckpointAsync().AsTask().GetAwaiter().GetResult();
        Console.WriteLine($"Store name: {_storeName}, status: {st}");
    }
}