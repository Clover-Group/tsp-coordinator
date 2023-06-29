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
    public string TableName { get; set; } = default! ;
    public EventSchema RowSchema { get; set; } = default! ;
    public string JdbcUrl { get; set; } = default! ;
    public string DriverName { get; set; } = default! ;
    public string? Password { get; set; }
    public int? BatchInterval { get; set; }
    public string? UserName { get; set; }
    public int? Parallelism { get; set; }
}

public class KafkaOutputConf : IOutputConf
{
    [JsonRequired(RequirementPolicy.Always)]
    public string Broker { get; set; } = default! ;
    public string Topic { get; set; } = default! ;
    public string? Serializer { get; set; }
    public int? Parallelism { get; set; }
    public EventSchema RowSchema { get; set; } = default! ;
}