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
        _healthCheckTimer = new Timer(HealthCheck, null, healthCheckInterval * 2, healthCheckInterval);
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
                    && (x.TotalJobCount) < _configurationService.MaxJobsPerTsp
                );
    }


    public Task<TspInstance[]> GetInstancesAsync()
    {
        return Task.FromResult(instances.ToArray());
    }

    public Task<(int, int)> GetInstancesCountAsync()
    {
        return Task.FromResult((instances.Count(), instances.Where(i => i.Status == TspInstanceStatus.Active).Count()));
    }

    public async void HealthCheck(Object? state)
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
                var response = await client.SendAsync(getVersionRequest);
                var responseBody = await response.Content.ReadAsStringAsync();
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
                instance.Status = TspInstanceStatus.Busy;
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
                var getJobsRequest = new HttpRequestMessage(HttpMethod.Get, tspGetJobsUrl);
                try
                {
                    var response = await client.SendAsync(getJobsRequest);
                    if (response.IsSuccessStatusCode)
                    {
                        var jobsIds = await response.Content.ReadFromJsonAsync<List<String>>();
                        instance.RunningJobsIds = jobsIds ?? new List<string>();
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
            }
        }
        foreach (var instance in instancesToRemove)
        {
            instances.Remove(instance);
        }
    }
}
