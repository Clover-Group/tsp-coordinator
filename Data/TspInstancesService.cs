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

    public bool AddInstance(TspInstance instance)
    {
        if (instances.Exists(i => i.Location == instance.Location)) 
        {
            return false;
        }
        instance.HealthCheckAttemptsRemaining = _configurationService.HealthCheckAttempts;
        instances.Add(instance);
        return true;
    }

    public TspInstance? FindFirstFreeInstance() => 
        instances.FirstOrDefault(x => x.Status == TspInstanceStatus.Active && x.RunningJobsIds.Count < _configurationService.MaxJobsPerTsp);

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
        foreach(var instance in currentInstances)
        {
            var tspGetVersionUrl = $"http://{instance.Host.MapToIPv4()}:{instance.Port}/metainfo/getVersion";
            var getVersionRequest = new HttpRequestMessage(HttpMethod.Get, tspGetVersionUrl);
            try
            {
                var response = await client.SendAsync(getVersionRequest);
                if (response.IsSuccessStatusCode)
                {
                    instance.Status = TspInstanceStatus.Active;
                    instance.HealthCheckAttemptsRemaining = _configurationService.HealthCheckAttempts;
                }
                else
                {
                    instance.Status = TspInstanceStatus.NotWorking;
                    instance.HealthCheckAttemptsRemaining--;
                }
            }
            catch (HttpRequestException ex)
            {
                instance.Status = TspInstanceStatus.NotResponding;
                instance.HealthCheckAttemptsRemaining--;
            }
            instance.HealthCheckDate = DateTime.Now;
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
                    }
                    else
                    {
                        instance.Status = TspInstanceStatus.CannotGetExtendedInfo;
                    }                   
                }
                catch (HttpRequestException ex)
                {
                    instance.Status = TspInstanceStatus.CannotGetExtendedInfo;
                }
            }
        }
        foreach(var instance in instancesToRemove)
        {
            instances.Remove(instance);
        }
    }
}
