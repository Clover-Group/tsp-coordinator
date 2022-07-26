using Dahomey.Json.Attributes;

namespace TspCoordinator.Data.TspApi.V2;

public interface IInputConf
{
    public int SourceId { get; set; }
    public string DatetimeField { get; set; }
    public List<string> PartitionFields { get; set; }
    public string? UnitIdField { get; set; }
    public int? Parallelism { get; set; }
    public int? NumParallelSources { get; set; }
    public int? PatternsParallelism { get; set; }
    public long? EventsMaxGapMs { get; set; }
    public long? DefaultEventsGapMs { get; set; }
    public long? ChunkSizeMs { get; set; }
    public ISourceDataTransformation? DataTransformation { get; set; }
    public double? DefaultToleranceFraction { get; set; }
}

public class JdbcInputConf : IInputConf
{
    public int SourceId { get; set; }
    [JsonRequired(RequirementPolicy.Always)]
    public String JdbcUrl { get; set; }

    public String Query { get; set; }
    public String DriverName { get; set; }
    public String DatetimeField { get; set; }

    public long? EventsMaxGapMs { get; set; }
    public long? DefaultEventsGapMs { get; set; }
    public long? ChunkSizeMs { get; set; }
    public List<string> PartitionFields { get; set; }
    public string? UnitIdField { get; set; }
    public String UserName { get; set; }
    public String Password { get; set; }
    public ISourceDataTransformation? DataTransformation { get; set; }
    public double? DefaultToleranceFraction { get; set; }
    public int? Parallelism { get; set; }
    public int? NumParallelSources { get; set; }
    public int? PatternsParallelism { get; set; }
    public double? TimestampMultiplier { get; set; }
}

public class KafkaInputConf : IInputConf
{
    public int SourceId { get; set; }
    [JsonRequired(RequirementPolicy.Always)]
    public string Brokers { get; set; }
    public string Topic { get; set; }
    public string Group { get; set; }
    public string? Serializer { get; set; }
    public string DatetimeField { get; set; }
    public List<string> PartitionFields { get; set; }
    public string? UnitIdField { get; set; }
    public ISourceDataTransformation? DataTransformation { get; set; }
    public double? TimestampMultiplier { get; set; }
    public long? EventsMaxGapMs { get; set; }
    public long? ChunkSizeMs { get; set; }
    public int? NumParallelSources { get; set; }
    public Dictionary<String, String> FieldsTypes { get; set; }
    public long? DefaultEventsGapMs { get; set; }
    public double? DefaultToleranceFraction { get; set; }

    public int? Parallelism { get; set; }
    public int? PatternsParallelism { get; set; }
}
