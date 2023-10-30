namespace TspCoordinator.Data;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Dahomey.Json;
using Prometheus;
using TspCoordinator.Controllers;

public class PriorityComparer : IComparer<int>
{
    public int Compare(int x, int y) => -x.CompareTo(y);
}

public enum JobStopResult
{
    Dequeued,
    StopRequested,
    NotFound
}

public class JobService
{
    private JobQueue jobQueue;

    private List<Job> runningJobs = new List<Job>();


    private List<Job> completedJobs = new List<Job>();

    private IHttpClientFactory _clientFactory;
    private Timer _queueTimer;
    private Timer _jobStateTimer;

    private Timer _cleanupTimer;

    private ILogger<JobService> _logger;

    private TspInstancesService _instancesService;

    private JobStatusReportingService _statusReportingService;

    private ConfigurationService _configurationService;

    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    }.SetupExtensions();

    private readonly Counter rowsReadCounter = 
        Metrics.CreateCounter("tsp_coordinator_rows_read", "Rows read by a job", "job_id");
    private readonly Counter rowsWrittenCounter =
        Metrics.CreateCounter("tsp_coordinator_rows_written", "Rows written by a job", "job_id");

    private readonly Gauge jobReadSpeedGauge =
        Metrics.CreateGauge("tsp_coordinator_job_read_speed", "Job read speed (rows per second)", "job_id");

    private readonly Gauge jobWriteSpeedGauge =
        Metrics.CreateGauge("tsp_coordinator_job_write_speed", "Job write speed (rows per second)", "job_id");

    private readonly Gauge jobCountsByStatusGauge =
        Metrics.CreateGauge("tsp_coordinator_job_count", "Job count (by status)", "status");

    public JobService(IHttpClientFactory clientFactory,
                      ILogger<JobService> logger,
                      TspInstancesService instancesService,
                      JobStatusReportingService statusReportingService,
                      ConfigurationService configurationService
                      )
    {
        _clientFactory = clientFactory;
        _logger = logger;
        _instancesService = instancesService;
        _instancesService.TspInstanceFailed += OnInstanceFailed;
        _instancesService.TspInstanceHealthCheckSucceeded += OnInstanceHealthCheckSucceeded;
        _statusReportingService = statusReportingService;
        _configurationService = configurationService;
        var queueInspectionInterval = (int)configurationService.QueueInspectionInterval;
        _queueTimer = new Timer(InspectQueue, null, queueInspectionInterval / 2, queueInspectionInterval);
        _jobStateTimer = new Timer(UpdateJobStates, null, queueInspectionInterval / 2, queueInspectionInterval);
        _cleanupTimer = new Timer(CleanupCompletedJobs, null, queueInspectionInterval / 2, queueInspectionInterval);
        foreach (var c in TspCoordinator.Data.TspApi.JsonConverters.Converters)
        {
            jsonOptions.Converters.Add(c);
        }
        jobQueue = new JobQueue(_configurationService.QueueStorageRedisSettings?.Host, "tsp-coordinator-queue");
    }

    public Task<List<Job>> GetJobQueueAsync()
    {
        return Task.FromResult(jobQueue.Jobs.ToList());
    }

    public Task<List<Job>> GetRunningJobsAsync()
    {
        return Task.FromResult(runningJobs);
    }

    public Task<List<Job>> GetCompletedQueueAsync()
    {
        return Task.FromResult(completedJobs);
    }

    public async Task<List<Job>> GetAllJobsAsync()
    {
        return (await GetJobQueueAsync()).Concat(await GetRunningJobsAsync()).Concat(await GetCompletedQueueAsync()).ToList();
    }

    public void EnqueueJob(Job job)
    {
        job.Lifecycle.AddQueued();
        jobQueue.Enqueue(job);
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
            job.NotifyStatusChanged();
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
            if (job.Status != JobStatus.Canceled)
            {
                job.Status = info.Success ? JobStatus.Finished : JobStatus.Failed;
                job.NotifyStatusChanged();
            }
            job.Lifecycle.AddFinished(info.Success, info.Error);
            UpdateJobMetric(job, new() { RowsRead = info.RowsRead ?? 0, RowsWritten = info.RowsWritten ?? 0 });
            lock (runningJobs) runningJobs.Remove(job);
            lock (completedJobs) completedJobs.Add(job);
            _statusReportingService.SendJobStatus(job, $"Job {job.JobId} completed.");
            // if job completed quickly (between health checks), we remove it from sent ones manually
            job.RunningOn?.SentJobsIds?.RemoveAll(x => x == job.JobId);
        }
    }

    public void OnInstanceFailed(TspInstance instance)
    {
        var jobsRunningOnFailedInstance = runningJobs.Where(j => j.RunningOn == instance).ToList();
        foreach (var job in jobsRunningOnFailedInstance)
        {
            lock (runningJobs) runningJobs.Remove(job);
            job.RunningOn = null;
            job.Status = JobStatus.Enqueued;
            job.NotifyStatusChanged();
            jobQueue.Enqueue(job);
        }
    }

    private async void OnInstanceHealthCheckSucceeded(TspInstance instance)
    {
        var registeredJobsForInstance = runningJobs.Where(j => j.RunningOn == instance).Select(j => j.JobId);
        var externalJobsIds = instance.RunningJobsIds.Where(id => !registeredJobsForInstance.Contains(id));
        foreach (var jobId in externalJobsIds)
        {
            var jobGetRequestUrl = $"http://{instance.Host.MapToIPv4()}:{instance.Port}/job/{jobId}/request";
            var jobGetRequestRequest = new HttpRequestMessage(HttpMethod.Get, jobGetRequestUrl);
            var client = _clientFactory.CreateClient("TspExternalJobRetriever");
            try
            {
                var response = await client.SendAsync(jobGetRequestRequest);
                if (response.IsSuccessStatusCode)
                {
                    var job = new Job
                    {
                        IsExternal = true,
                        JobId = jobId,
                        RunningOn = instance,
                        Status = JobStatus.Running
                    };
                    job.Lifecycle.AddExternalDiscovered();
                    lock (runningJobs) runningJobs.Add(job);

                }
                else
                {
                    // TODO
                }
            }
            catch (HttpRequestException)
            {
                // TODO:
            }
        }
    }

    public async void UpdateJobStates(Object? state)
    {
        List<Job> jobsCopy;
        lock (runningJobs)
        {
            jobsCopy = runningJobs.ToList();
        }
        foreach (var job in jobsCopy)
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
                        UpdateJobMetric(job, statusInfo ?? new JobStatusInfo());
                        
                    }
                    else
                    {
                        // TODO
                    }
                }
                catch (HttpRequestException)
                {
                    // TODO:
                }
                catch (TaskCanceledException)
                {
                    // TSP is busy, do nothing (yet)
                    _logger.LogWarning($"TSP is probably busy, no update for job {job.JobId} yet");
                }
            }
        }
    }

    public async void InspectQueue(Object? state)
    {
        try
        {
            var jobCounts = JobCountsByStatus();
            foreach(var (status, count) in jobCounts)
            {
                jobCountsByStatusGauge.WithLabels(status.ToString().ToUpper()).Set(count);
            }
            //Console.WriteLine($"Inspecting queue: {jobQueue.Jobs.Count} jobs found");
            while (jobQueue.Jobs.Count > 0)
            {
                var firstFreeInstance = _instancesService.FindFirstFreeInstance();

                if (firstFreeInstance == null) return;

                var job = jobQueue.Dequeue()!;
                job.RunningOn = firstFreeInstance;
                firstFreeInstance.SentJobsIds.Add(job.JobId);
                lock (runningJobs) runningJobs.Add(job);

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
                    _logger.LogInformation($"Job {job.JobId} sent, response code is {(int)response.StatusCode} with {await response.Content.ReadAsStringAsync()}");
                    if (!response.IsSuccessStatusCode)
                    {
                        // TODO: Failed to send job
                        _logger.LogCritical($"Failed to send job {job.JobId}, returned status {(int)response.StatusCode} with {await response.Content.ReadAsStringAsync()}");
                        lock (runningJobs) runningJobs.Remove(job);
                        job.Status = JobStatus.Failed;
                        job.NotifyStatusChanged();
                        job.Lifecycle.AddLogMessage($"Failed to send job {job.JobId}, returned status {(int)response.StatusCode} with {await response.Content.ReadAsStringAsync()}");
                        _statusReportingService.SendJobStatus(job, $"Job {job.JobId} not started because of TSP failure (HTTP error {(int)response.StatusCode})");
                        lock (completedJobs) completedJobs.Add(job);
                    }
                }
                catch (HttpRequestException ex)
                {
                    // TODO:
                    _logger.LogCritical($"Failed to send job {job.JobId}, an exception occurred: {ex.Message}");
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning($"An exception: `{e.Message}` occurred during queue inspection, skipping that schedule...");
        }
    }

    public async Task<JobStopResult> StopJob(string jobId)
    {
        if (jobQueue.FindById(jobId) is Job job)
        {
            job.Status = JobStatus.Canceled;
            job.NotifyStatusChanged();
            _statusReportingService.SendJobStatus(job, $"Job {jobId} was canceled before start and dequeued");
            jobQueue.RemoveById(jobId);
            completedJobs.Add(job);
            return JobStopResult.Dequeued;
        }
        var findInRunning = runningJobs.Find(j => j.JobId == jobId);
        if (findInRunning != null)
        {
            var client = _clientFactory.CreateClient("TspJobStopper");
            var instance = findInRunning.RunningOn;
            if (instance == null)
            {
                return JobStopResult.NotFound;
            }
            var jobStopUrl = $"http://{instance.Host.MapToIPv4()}:{instance.Port}/job/{jobId}/stop/";
            var response = await client.PostAsync(jobStopUrl, null);
            // TODO: Handle response
            findInRunning.Status = JobStatus.Canceled;
            findInRunning.NotifyStatusChanged();
            _statusReportingService.SendJobStatus(findInRunning, $"Job {findInRunning.JobId} was canceled");
            return JobStopResult.StopRequested;
        }
        return JobStopResult.NotFound;
    }

    public void CleanupCompletedJobs(Object? state)
    {
        var cleanupCompletedInterval = _configurationService.CleanupCompletedInterval;
        var now = DateTime.Now;
        var jobIdsToRemove = completedJobs
            .Where(job => (now - job.Lifecycle.Events.LastOrDefault().Key).TotalMilliseconds > cleanupCompletedInterval)
            .Select(job => job.JobId)
            .ToArray();
        foreach (var id in jobIdsToRemove)
        {
            rowsReadCounter.RemoveLabelled(id);
            rowsWrittenCounter.RemoveLabelled(id);
            jobReadSpeedGauge.RemoveLabelled(id);
            jobWriteSpeedGauge.RemoveLabelled(id);
        }
        completedJobs.RemoveAll(job => jobIdsToRemove.Contains(job.JobId));
    }

    public Job? FindJobById(string id) =>
        jobQueue.FindById(id)
            ?? runningJobs.Find(j => j.JobId == id)
            ?? completedJobs.Find(j => j.JobId == id);

    public Dictionary<JobStatus, int> JobCountsByStatus()
    {
        return new[] {
            jobQueue.GetCountsByStatus(),
            runningJobs.GroupBy(job => job.Status).ToDictionary(group => group.Key, group => group.Count()),
            completedJobs.GroupBy(job => job.Status).ToDictionary(group => group.Key, group => group.Count())
        }.SelectMany(c => c).ToLookup(p => p.Key, p => p.Value).ToDictionary(g => g.Key, g => g.Sum());
    }

    private void UpdateJobMetric(Job job, JobMetricPoint point)
    {
        job.MetricHistory.Add(point);
        rowsReadCounter.WithLabels(job.JobId).IncTo(point.RowsRead);
        rowsWrittenCounter.WithLabels(job.JobId).IncTo(point.RowsWritten);
        job.CacheSpeed();
        var speed = job.Speed;
        jobReadSpeedGauge.WithLabels(job.JobId).Set(speed.Item1);
        jobWriteSpeedGauge.WithLabels(job.JobId).Set(speed.Item2);
    }
}