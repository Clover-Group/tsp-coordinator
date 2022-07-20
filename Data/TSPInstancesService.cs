namespace TSPCoordinator.Data;

public class TSPInstancesService
{

    private List<TSPInstance> instances = new List<TSPInstance>
    {
        new TSPInstance 
        {
            Host = "10.85.0.1", 
            Port = 8080,
            Version = "19.0.0",
            Status = TSPInstanceStatus.Active,
            HealthCheckDate = DateTime.Now
        },
        new TSPInstance 
        {
            Host = "10.85.0.2",
            Port = 8080,
            Version = "19.0.0",
            Status = TSPInstanceStatus.Active,
            HealthCheckDate = DateTime.Now
        },
        new TSPInstance
        {
            Host = "10.86.0.1",
            Port = 8080,
            Version = "19.0.0",
            Status = TSPInstanceStatus.NotWorking,
            HealthCheckDate = DateTime.Now
        }
    };

    public void AddInstance(TSPInstance instance)
    {
        instances.Add(instance);
    }

    public Task<TSPInstance[]> GetInstancesAsync()
    {
        return Task.FromResult(instances.ToArray());
    }

    public Task<(int, int)> GetInstancesCountAsync()
    {
        return Task.FromResult((instances.Count(), instances.Where(i => i.Status == TSPInstanceStatus.Active).Count()));
    }
}
