using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace TspCoordinator.Data;

public class TspInstancesService
{
    private readonly IHttpClientFactory _clientFactory;

    private readonly Timer _healthCheckTimer;

    private ConfigurationService _configurationService;

    public TspInstancesService(IHttpClientFactory clientFactory, ConfigurationService configurationService)
    {
        _clientFactory = clientFactory;
        _configurationService = configurationService;
        var healthCheckInterval = (int)_configurationService.HealthCheckInterval;
        _healthCheckTimer = new Timer(
            HealthCheck, null,
            Math.Max(-1, healthCheckInterval * 2),
            Math.Max(-1, healthCheckInterval)
        );
    }

    private List<TspInstance> instances = new List<TspInstance> { };

    public delegate void TspInstanceFailedHandler(TspInstance instance);

    public event TspInstanceFailedHandler? TspInstanceFailed;

    public delegate void TspInstanceHealthCheckSucceededHandler(TspInstance instance);

    public event TspInstanceHealthCheckSucceededHandler? TspInstanceHealthCheckSucceeded;

    public bool AddInstance(TspInstance instance)
    {
        if (instances.FirstOrDefault(i => i.Uuid == instance.Uuid) is TspInstance foundInstance)
        {
            foundInstance.Version = instance.Version;
            return false;
        }
        instance.HealthCheckAttemptsRemaining = _configurationService.HealthCheckAttempts;
        instances.Add(instance);
        return true;
    }

    public TspInstance? FindFirstFreeInstance()
    {
        var copiedInstances = new List<TspInstance>(instances);
        return copiedInstances
            .OrderBy(x => x.TotalJobCount)
            .FirstOrDefault(
                x => x.Status == TspInstanceStatus.Active
                    && x.TotalSentJobsCounter < x.TotalJobsLimit
                    && x.TotalJobCount < _configurationService.MaxJobsPerTsp
                );
    }


    public Task<TspInstance[]> GetInstancesAsync()
    {
        return Task.FromResult(instances.ToArray());
    }

    public Task<(int, int)> GetInstancesCountAsync()
    {
        return Task.FromResult((instances.Count, instances.Where(i => i.Status == TspInstanceStatus.Active).Count()));
    }

    public void HealthCheck(Object? state)
    {
        var client = _clientFactory.CreateClient("TspHealthChecker");
        var instancesToRemove = new List<TspInstance>();
        var currentInstances = new List<TspInstance>(instances);
        foreach (var instance in currentInstances)
        {
            var tspGetVersionUrl = $"http://{instance.Host.MapToIPv4()}:{instance.Port}/metainfo/getVersion";
            var getVersionRequest = new HttpRequestMessage(HttpMethod.Get, tspGetVersionUrl);
            try
            {
                var response = client.Send(getVersionRequest);
                var responseBody = response.Content.ReadAsStringAsync().Result;
                if (response.IsSuccessStatusCode && responseBody.Contains(instance.Uuid.ToString()))
                {
                    instance.Status = TspInstanceStatus.Active;
                    instance.HealthCheckAttemptsRemaining = _configurationService.HealthCheckAttempts;
                }
                else
                {
                    instance.Status = TspInstanceStatus.NotWorking;
                    instance.HealthCheckAttemptsRemaining--;
                    instance.SentJobsIds.Clear();
                }
                instance.HealthCheckDate = DateTime.Now;
            }
            catch (HttpRequestException)
            {
                instance.Status = TspInstanceStatus.NotResponding;
                instance.HealthCheckAttemptsRemaining--;
                instance.HealthCheckDate = DateTime.Now;
            }
            catch (TaskCanceledException)
            {
                if (_configurationService.TreatBusyAsNotResponding)
                {
                    instance.Status = TspInstanceStatus.NotResponding;
                    instance.HealthCheckAttemptsRemaining--;
                    instance.HealthCheckDate = DateTime.Now;
                }
                else
                {
                    instance.Status = TspInstanceStatus.Busy;
                }
            }
            if (instance.HealthCheckAttemptsRemaining == 0)
            {
                TspInstanceFailed?.Invoke(instance);
                instancesToRemove.Add(instance);
                continue;
            }
            if (instance.Status == TspInstanceStatus.Active)
            {
                var tspGetJobsUrl = $"http://{instance.Host.MapToIPv4()}:{instance.Port}/jobs/overview";
                var tspGetLimitsUrl = $"http://{instance.Host.MapToIPv4()}:{instance.Port}/jobs/limits";
                var getJobsRequest = new HttpRequestMessage(HttpMethod.Get, tspGetJobsUrl);
                var getLimitsRequest = new HttpRequestMessage(HttpMethod.Get, tspGetLimitsUrl);
                try
                {
                    var response = client.Send(getJobsRequest);
                    if (response.IsSuccessStatusCode)
                    {
                        var jobsIds = response.Content.ReadFromJsonAsync<List<String>>().GetAwaiter().GetResult();
                        instance.RunningJobsIds = jobsIds ?? [];
                        instance.SentJobsIds.RemoveAll(x => instance.RunningJobsIds?.Contains(x) ?? false);
                        TspInstanceHealthCheckSucceeded?.Invoke(instance);
                    }
                    else
                    {
                        instance.Status = TspInstanceStatus.CannotGetExtendedInfo;
                        instance.SentJobsIds.Clear();
                    }
                }
                catch (HttpRequestException)
                {
                    instance.Status = TspInstanceStatus.CannotGetExtendedInfo;
                    instance.SentJobsIds.Clear();
                }
                if (instance.SupportsCapability(TspCapability.TotalJobsLimit))
                {
                    try
                    {
                        var response = client.Send(getLimitsRequest);
                        if (response.IsSuccessStatusCode)
                        {
                            var counters = response.Content.ReadFromJsonAsync<List<int>>().GetAwaiter().GetResult();
                            if (counters[0] > instance.TotalSentJobsCounter) instance.TotalSentJobsCounter = counters[0];
                            if (counters[1] > instance.TotalFinishedJobsCounter) instance.TotalFinishedJobsCounter = counters[1];
                            if (counters[2] > instance.TotalJobsLimit) instance.TotalJobsLimit = counters[2];

                            if (instance.TotalSentJobsCounter >= instance.TotalJobsLimit)
                            {
                                instance.Status = TspInstanceStatus.RestartScheduled;
                            }
                        }
                        else
                        {
                            // instance.TotalSentJobsCounter = 0;
                            // instance.TotalFinishedJobsCounter = 0;
                            instance.TotalJobsLimit = Int32.MaxValue;
                        }
                    }
                    catch (Exception)
                    {
                        // instance.TotalSentJobsCounter = 0;
                        // instance.TotalFinishedJobsCounter = 0;
                        instance.TotalJobsLimit = Int32.MaxValue;
                    }
                }
            }
        }
        foreach (var instance in instancesToRemove)
        {
            instances.Remove(instance);
        }
    }
}
