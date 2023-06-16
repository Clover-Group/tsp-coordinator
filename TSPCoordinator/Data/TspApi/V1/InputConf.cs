using Dahomey.Json.Attributes;

namespace TspCoordinator.Data.TspApi.V1;

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
    public String JdbcUrl { get; set; } = default! ;

    public String Query { get; set; } = default! ;
    public String DriverName { get; set; } = default! ;
    public String DatetimeField { get; set; } = default! ;

    public long? EventsMaxGapMs { get; set; }
    public long? DefaultEventsGapMs { get; set; }
    public long? ChunkSizeMs { get; set; }
    public List<string> PartitionFields { get; set; } = default! ;
    public string? UnitIdField { get; set; }
    public String UserName { get; set; } = default! ;
    public String Password { get; set; } = default! ;
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

    public string Brokers { get; set; } = default! ;
    public string Topic { get; set; } = default! ;
    public string Group { get; set; } = default! ;
    public string? Serializer { get; set; }
    public string DatetimeField { get; set; } = default! ;
    public List<string> PartitionFields { get; set; } = default! ;
    public string? UnitIdField { get; set; }
    public ISourceDataTransformation? DataTransformation { get; set; }
    public double? TimestampMultiplier { get; set; }
    public long? EventsMaxGapMs { get; set; }
    public long? ChunkSizeMs { get; set; }
    public int? NumParallelSources { get; set; }
    public Dictionary<String, String> FieldsTypes { get; set; } = default! ;
    public long? DefaultEventsGapMs { get; set; }
    public double? DefaultToleranceFraction { get; set; }

    public int? Parallelism { get; set; }
    public int? PatternsParallelism { get; set; }
}
