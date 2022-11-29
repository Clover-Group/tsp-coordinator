using Dahomey.Json.Attributes;

namespace TspCoordinator.Data.TspApi.V1;

public interface IOutputConf
{
    public int? Parallelism { get; set; }
    public EventSchema RowSchema { get; set; }
}

public class JdbcOutputConf : IOutputConf
{
    [JsonRequired(RequirementPolicy.Always)]
    public string TableName { get; set; }
    public EventSchema RowSchema { get; set; }
    public string JdbcUrl { get; set; }
    public string DriverName { get; set; }
    public string? Password { get; set; }
    public int? BatchInterval { get; set; }
    public string? UserName { get; set; }
    public int? Parallelism { get; set; }
}

public class KafkaOutputConf : IOutputConf
{
    [JsonRequired(RequirementPolicy.Always)]
    public string Broker { get; set; }
    public string Topic { get; set; }
    public string? Serializer { get; set; }
    public int? Parallelism { get; set; }
    public EventSchema RowSchema { get; set; }
}