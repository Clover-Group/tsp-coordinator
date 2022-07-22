namespace TspCoordinator.Data;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
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

    private IHttpClientFactory _clientFactory;
    private Timer _queueTimer;

    private ILogger<JobService> _logger;

    private TspInstancesService _instancesService;

    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };


    public JobService(IHttpClientFactory clientFactory, ILogger<JobService> logger, TspInstancesService instancesService)
    {
        _clientFactory = clientFactory;
        _logger = logger;
        _instancesService = instancesService;
        _queueTimer = new Timer(InspectQueue, null, 10000, 5000);
        foreach (var c in TspCoordinator.Data.TspApi.JsonConverters.Converters)
        {
            jsonOptions.Converters.Add(c);
        }
    }

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

    public void EnqueueJob(Job job)
    {
        jobQueue.Enqueue(job, job.Request?.Priority ?? 0);
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
        var job = runningJobs.Find(x => x.JobId == info.JobId);
        if (job == null)
        {

        }
        else
        {
            job.Status = info.Success ? JobStatus.Finished : JobStatus.Failed;
            runningJobs.Remove(job);
            completedJobs.Add(job);
        }
    }

    public async void InspectQueue(Object? state)
    {
        if (jobQueue.Count == 0) return;

        var firstFreeInstance = _instancesService.FindFirstFreeInstance();

        if (firstFreeInstance == null) return;

        var job = jobQueue.Dequeue();
        job.RunningOn = firstFreeInstance;
        runningJobs.Add(job);

        var jobSubmitUrl = $"http://{firstFreeInstance.Host.MapToIPv4()}:{firstFreeInstance.Port}/job/submit/";

        var client = _clientFactory.CreateClient("TspJobRunner");
        try
        {
            var requestAsJson = JsonSerializer.Serialize(job.Request, jsonOptions);
            _logger.LogInformation(requestAsJson);
            var response = await client.PostAsync(jobSubmitUrl,
                new StringContent(
                    requestAsJson,
                    Encoding.UTF8,
                    "application/json")
            );
            if (!response.IsSuccessStatusCode)
            {
                // TODO: Failed to send job
                _logger.LogCritical($"Failed to send job {job.JobId}, returned status {response.StatusCode} with {await response.Content.ReadAsStringAsync()}");
                runningJobs.Remove(job);
                job.Status = JobStatus.Canceled;
                completedJobs.Add(job);
            }
        }
        catch (HttpRequestException ex)
        {
            // TODO:
        }

    }
}