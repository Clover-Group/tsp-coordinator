namespace TSPCoordinator.Data;

public enum TSPInstanceStatus {
    Active,
    NotWorking
}

public class TSPInstance
{
    

    public string Host { get; set; }

    public int Port { get; set; }

    public string Location => $"{Host}:{Port}";

    public string? Version { get; set; }

    public TSPInstanceStatus Status { get; set; }

    public DateTime HealthCheckDate { get; set; }
}
