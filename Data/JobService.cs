namespace TspCoordinator.Data;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Dahomey.Json;
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
    private Timer _jobStateTimer;

    private ILogger<JobService> _logger;

    private TspInstancesService _instancesService;

    private JobStatusReportingService _statusReportingService;

    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    }.SetupExtensions();


    public JobService(IHttpClientFactory clientFactory, ILogger<JobService> logger, TspInstancesService instancesService, JobStatusReportingService statusReportingService)
    {
        _clientFactory = clientFactory;
        _logger = logger;
        _instancesService = instancesService;
        _statusReportingService = statusReportingService;
        _queueTimer = new Timer(InspectQueue, null, 10000, 5000);
        _jobStateTimer = new Timer(UpdateJobStates, null, 10000, 5000);
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
        _statusReportingService.SendJobStatus(job, $"Job {job.JobId} enqueued.");
    }

    public void OnJobStarted(JobStartedInfo info)
    {
        var job = runningJobs.Find(x => x.JobId == info.JobId);
        if (job == null)
        {
            // TODO: if no job was registered
        }
        else
        {
            job.Status = JobStatus.Running;
            _statusReportingService.SendJobStatus(job, $"Job {job.JobId} successfully started.");
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
            job.RowsRead = info.RowsRead ?? 0;
            job.RowsWritten = info.RowsWritten ?? 0;
            runningJobs.Remove(job);
            completedJobs.Add(job);
            _statusReportingService.SendJobStatus(job, $"Job {job.JobId} completed.");
        }
    }

    public async void UpdateJobStates(Object? state)
    {
        foreach (var job in runningJobs.ToList())
        {
            if (job.RunningOn != null)
            {
                var instance = job.RunningOn;
                var jobStatusUrl = $"http://{instance.Host.MapToIPv4()}:{instance.Port}/job/{job.JobId}/status";
                var jobStatusRequest = new HttpRequestMessage(HttpMethod.Get, jobStatusUrl);

                var client = _clientFactory.CreateClient("TspJobStatusChecker");
                try
                {
                    var response = await client.SendAsync(jobStatusRequest);
                    if (response.IsSuccessStatusCode)
                    {
                        var statusInfo = await response.Content.ReadFromJsonAsync<JobStatusInfo>();
                        job.RowsRead = statusInfo?.RowsRead ?? -1;
                        job.RowsWritten = statusInfo?.RowsWritten ?? -1;
                    }
                    else
                    {
                        // TODO
                    }
                }
                catch (HttpRequestException ex)
                {
                    // TODO:
                }
            }
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
            //_logger.LogInformation(requestAsJson);
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