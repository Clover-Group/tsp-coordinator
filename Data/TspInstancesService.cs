using System.Net.Http;

namespace TspCoordinator.Data;

public class TspInstancesService
{
    private readonly IHttpClientFactory _clientFactory;

    private readonly Timer _healthCheckTimer;

    public TspInstancesService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        _healthCheckTimer = new Timer(HealthCheck, null, 5000, 10000);
    }

    private List<TspInstance> instances = new List<TspInstance> { };

    public bool AddInstance(TspInstance instance)
    {
        if (instances.Exists(i => i.Location == instance.Location)) 
        {
            return false;
        }
        instances.Add(instance);
        return true;
    }

    public TspInstance? FindFirstFreeInstance() => instances.FirstOrDefault(x => x.Status == TspInstanceStatus.Active);

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
        foreach(var instance in instances)
        {
            var tspGetVersionUrl = $"http://{instance.Host.MapToIPv4()}:{instance.Port}/metainfo/getVersion";
            var getVersionRequest = new HttpRequestMessage(HttpMethod.Get, tspGetVersionUrl);
            try
            {
                var response = await client.SendAsync(getVersionRequest);
                if (response.IsSuccessStatusCode)
                {
                    instance.Status = TspInstanceStatus.Active;
                }
                else
                {
                    instance.Status = TspInstanceStatus.NotWorking;
                }
            }
            catch (HttpRequestException ex)
            {
                instance.Status = TspInstanceStatus.NotResponding;
            }
            instance.HealthCheckDate = DateTime.Now;
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
    }
}
