namespace TspCoordinator.Data.TspApi;

using Actual = V3;

public static class Converters
{

    public static Actual.Request ConvertRequestFromV1(V1.Request request) => new Actual.Request
    {
        Uuid = request.Uuid,
        Source = new Actual.SourceWithType
        {
            Type = GetTypeFromInputConfV1(request.Source),
            Config = ConvertInputConfFromV1(request.Source)
        },
        Sinks = new List<Actual.SinkWithType>
        {
            new Actual.SinkWithType
            {
                Type = GetTypeFromOutputConfV1(request.Sink),
                Config = ConvertOutputConfFromV1(request.Sink)
            }

        },
        Patterns = request.Patterns.Select(p => ConvertPatternFromV1(p)).ToList(),
        Priority = 0
    };

    public static Actual.Request ConvertRequestFromV2(V2.Request request) => new Actual.Request
    {
        Uuid = request.Uuid,
        Source = new Actual.SourceWithType
        {
            Type = GetTypeFromInputConfV2(request.Source),
            Config = ConvertInputConfFromV2(request.Source)
        },
        Sinks = new List<Actual.SinkWithType>
        {
            new Actual.SinkWithType
            {
                Type = GetTypeFromOutputConfV2(request.Sink),
                Config = ConvertOutputConfFromV2(request.Sink)
            }

        },
        Patterns = request.Patterns.Select(p => ConvertPatternFromV2(p)).ToList(),
        Priority = request.Priority
    };

    public static Actual.Request ConvertRequestFromV3(V3.Request request)
    {
        request.Source.Config = ConvertInputConfFromV3(request.Source.Config);
        foreach (var sink in request.Sinks)
        {
            sink.Config = ConvertOutputConfFromV3(sink.Config);
        }
        return request;
    }

    public static string GetTypeFromInputConfV1(V1.IInputConf inputConf) => inputConf switch
    {
        V1.JdbcInputConf _ => "jdbc",
        V1.KafkaInputConf _ => "kafka",
        _ => ""
    };

    public static string GetTypeFromInputConfV2(V2.IInputConf inputConf) => inputConf switch
    {
        V2.JdbcInputConf _ => "jdbc",
        V2.KafkaInputConf _ => "kafka",
        _ => ""
    };

    public static string GetTypeFromInputConfV3(V3.IInputConf inputConf) => inputConf switch
    {
        V3.JdbcInputConf _ => "jdbc",
        V3.KafkaInputConf _ => "kafka",
        _ => ""
    };

    public static string GetTypeFromOutputConfV1(V1.IOutputConf outputConf) => outputConf switch
    {
        V1.JdbcOutputConf _ => "jdbc",
        V1.KafkaOutputConf _ => "kafka",
        _ => ""
    };

    public static string GetTypeFromOutputConfV2(V2.IOutputConf outputConf) => outputConf switch
    {
        V2.JdbcOutputConf _ => "jdbc",
        V2.KafkaOutputConf _ => "kafka",
        _ => ""
    };

    public static string GetTypeFromOutputConfV3(V3.IOutputConf outputConf) => outputConf switch
    {
        V3.JdbcOutputConf _ => "jdbc",
        V3.KafkaOutputConf _ => "kafka",
        _ => ""
    };

    public static Actual.Pattern ConvertPatternFromV1(V1.Pattern pattern) => new Actual.Pattern
    {
        Id = Int32.TryParse(pattern.Id, out int patid) ? patid : -1,
        SourceCode = pattern.SourceCode,
        Metadata = pattern.Payload,
        Subunit = Convert.ToInt32(pattern.Payload?.GetValueOrDefault("subunit", "0"))
    };

    public static Actual.Pattern ConvertPatternFromV2(V2.Pattern pattern) => new Actual.Pattern
    {
        Id = pattern.Id,
        SourceCode = pattern.SourceCode,
        Metadata = pattern.Payload,
        Subunit = pattern.Subunit
    };

    public static Actual.Pattern ConvertPatternFromV3(V3.Pattern pattern) => pattern;

    public static Actual.ISourceDataTransformation? ConvertSDTFromV1(V1.ISourceDataTransformation? sdt)
        => sdt switch
        {
            V1.NarrowDataUnfolding ndu => new Actual.NarrowDataUnfolding
            {
                Config = new Actual.NarrowDataUnfoldingConf
                {
                    KeyColumn = (ndu.Config as V1.NarrowDataUnfoldingConf)?.KeyColumn ?? default!,
                    DefaultValueColumn = (ndu.Config as V1.NarrowDataUnfoldingConf)?.DefaultValueColumn ?? default!,
                    FieldsTimeoutsMs = (ndu.Config as V1.NarrowDataUnfoldingConf)?.FieldsTimeoutsMs ?? default!,
                    ValueColumnMapping = (ndu.Config as V1.NarrowDataUnfoldingConf)?.ValueColumnMapping,
                }
            },
            V1.WideDataFilling wdf => new Actual.WideDataFilling
            {
                Config = new Actual.WideDataFillingConf
                {
                    FieldsTimeoutsMs = (wdf.Config as V1.WideDataFillingConf)?.FieldsTimeoutsMs ?? default!,
                    DefaultTimeout = (wdf.Config as V1.WideDataFillingConf)?.DefaultTimeout ?? default!,
                }
            },
            _ => null
        };

    public static Actual.ISourceDataTransformation? ConvertSDTFromV2(V2.ISourceDataTransformation? sdt)
        => sdt switch
        {
            V2.NarrowDataUnfolding ndu => new Actual.NarrowDataUnfolding
            {
                Config = new Actual.NarrowDataUnfoldingConf
                {
                    KeyColumn = (ndu.Config as V2.NarrowDataUnfoldingConf)?.KeyColumn ?? default!,
                    DefaultValueColumn = (ndu.Config as V2.NarrowDataUnfoldingConf)?.DefaultValueColumn ?? default!,
                    FieldsTimeoutsMs = (ndu.Config as V2.NarrowDataUnfoldingConf)?.FieldsTimeoutsMs ?? default!,
                    ValueColumnMapping = (ndu.Config as V2.NarrowDataUnfoldingConf)?.ValueColumnMapping,
                }
            },
            V2.WideDataFilling wdf => new Actual.WideDataFilling
            {
                Config = new Actual.WideDataFillingConf
                {
                    FieldsTimeoutsMs = (wdf.Config as V2.WideDataFillingConf)?.FieldsTimeoutsMs ?? default!,
                    DefaultTimeout = (wdf.Config as V2.WideDataFillingConf)?.DefaultTimeout ?? default!,
                }
            },
            _ => null
        };

    public static Actual.ISourceDataTransformation ConvertSDTFromV3(V3.ISourceDataTransformation sdt) => sdt;

    public static Actual.IInputConf ConvertInputConfFromV1(V1.IInputConf inputConf)
        => inputConf switch
        {
            V1.JdbcInputConf jdbcInputConf =>
                new Actual.JdbcInputConf
                {
                    ChunkSizeMs = jdbcInputConf.ChunkSizeMs,
                    DataTransformation = ConvertSDTFromV1(jdbcInputConf.DataTransformation),
                    DatetimeField = jdbcInputConf.DatetimeField,
                    DefaultEventsGapMs = jdbcInputConf.DefaultEventsGapMs,
                    DefaultToleranceFraction = jdbcInputConf.DefaultToleranceFraction,
                    DriverName = jdbcInputConf.DriverName,
                    EventsMaxGapMs = jdbcInputConf.EventsMaxGapMs,
                    JdbcUrl = jdbcInputConf.JdbcUrl,
                    NumParallelSources = jdbcInputConf.NumParallelSources,
                    Parallelism = jdbcInputConf.Parallelism,
                    PartitionFields = jdbcInputConf.PartitionFields,
                    Password = jdbcInputConf.Password,
                    PatternsParallelism = jdbcInputConf.PatternsParallelism,
                    ProcessingBatchSize = null,
                    Query = jdbcInputConf.Query,
                    SourceId = jdbcInputConf.SourceId,
                    TimestampMultiplier = jdbcInputConf.TimestampMultiplier,
                    UnitIdField = jdbcInputConf.UnitIdField,
                    UserName = jdbcInputConf.UserName
                },
            V1.KafkaInputConf kafkaInputConf =>
                new Actual.KafkaInputConf
                {
                    Brokers = kafkaInputConf.Brokers,
                    ChunkSizeMs = kafkaInputConf.ChunkSizeMs,
                    DataTransformation = ConvertSDTFromV1(kafkaInputConf.DataTransformation),
                    DatetimeField = kafkaInputConf.DatetimeField,
                    DefaultEventsGapMs = kafkaInputConf.DefaultEventsGapMs,
                    DefaultToleranceFraction = kafkaInputConf.DefaultToleranceFraction,
                    EventsMaxGapMs = kafkaInputConf.EventsMaxGapMs,
                    FieldsTypes = kafkaInputConf.FieldsTypes,
                    Group = kafkaInputConf.Group,
                    NumParallelSources = kafkaInputConf.NumParallelSources,
                    Parallelism = kafkaInputConf.Parallelism,
                    PartitionFields = kafkaInputConf.PartitionFields,
                    PatternsParallelism = kafkaInputConf.PatternsParallelism,
                    ProcessingBatchSize = null,
                    Serializer = kafkaInputConf.Serializer,
                    SourceId = kafkaInputConf.SourceId,
                    TimestampMultiplier = kafkaInputConf.TimestampMultiplier,
                    Topic = kafkaInputConf.Topic,
                    UnitIdField = kafkaInputConf.UnitIdField
                },
            _ => throw new ArgumentException($"Unsupported input configuration: {inputConf} was provided")
        };

    public static Actual.IInputConf ConvertInputConfFromV2(V2.IInputConf inputConf)
        => inputConf switch
        {
            V2.JdbcInputConf jdbcInputConf =>
                new Actual.JdbcInputConf
                {
                    ChunkSizeMs = jdbcInputConf.ChunkSizeMs,
                    DataTransformation = ConvertSDTFromV2(jdbcInputConf.DataTransformation),
                    DatetimeField = jdbcInputConf.DatetimeField,
                    DefaultEventsGapMs = jdbcInputConf.DefaultEventsGapMs,
                    DefaultToleranceFraction = jdbcInputConf.DefaultToleranceFraction,
                    DriverName = jdbcInputConf.DriverName,
                    EventsMaxGapMs = jdbcInputConf.EventsMaxGapMs,
                    JdbcUrl = jdbcInputConf.JdbcUrl,
                    NumParallelSources = jdbcInputConf.NumParallelSources,
                    Parallelism = jdbcInputConf.Parallelism,
                    PartitionFields = jdbcInputConf.PartitionFields,
                    Password = jdbcInputConf.Password,
                    PatternsParallelism = jdbcInputConf.PatternsParallelism,
                    ProcessingBatchSize = null,
                    Query = jdbcInputConf.Query,
                    SourceId = jdbcInputConf.SourceId,
                    TimestampMultiplier = jdbcInputConf.TimestampMultiplier,
                    UnitIdField = jdbcInputConf.UnitIdField,
                    UserName = jdbcInputConf.UserName
                },
            V2.KafkaInputConf kafkaInputConf =>
                new Actual.KafkaInputConf
                {
                    Brokers = kafkaInputConf.Brokers,
                    ChunkSizeMs = kafkaInputConf.ChunkSizeMs,
                    DataTransformation = ConvertSDTFromV2(kafkaInputConf.DataTransformation),
                    DatetimeField = kafkaInputConf.DatetimeField,
                    DefaultEventsGapMs = kafkaInputConf.DefaultEventsGapMs,
                    DefaultToleranceFraction = kafkaInputConf.DefaultToleranceFraction,
                    EventsMaxGapMs = kafkaInputConf.EventsMaxGapMs,
                    FieldsTypes = kafkaInputConf.FieldsTypes,
                    Group = kafkaInputConf.Group,
                    NumParallelSources = kafkaInputConf.NumParallelSources,
                    Parallelism = kafkaInputConf.Parallelism,
                    PartitionFields = kafkaInputConf.PartitionFields,
                    PatternsParallelism = kafkaInputConf.PatternsParallelism,
                    ProcessingBatchSize = null,
                    Serializer = kafkaInputConf.Serializer,
                    SourceId = kafkaInputConf.SourceId,
                    TimestampMultiplier = kafkaInputConf.TimestampMultiplier,
                    Topic = kafkaInputConf.Topic,
                    UnitIdField = kafkaInputConf.UnitIdField
                },
            _ => throw new ArgumentException($"Unsupported input configuration: {inputConf} was provided")
        };

    public static Actual.IInputConf ConvertInputConfFromV3(V3.IInputConf inputConf)
    {
        return inputConf;
    }

    public static Actual.IOutputConf ConvertOutputConfFromV1(V1.IOutputConf outputConf)
        => outputConf switch
        {
            V1.JdbcOutputConf jdbcOutputConf => new Actual.JdbcOutputConf
            {
                BatchInterval = jdbcOutputConf.BatchInterval,
                DriverName = jdbcOutputConf.DriverName,
                JdbcUrl = jdbcOutputConf.JdbcUrl,
                Parallelism = jdbcOutputConf.Parallelism,
                Password = jdbcOutputConf.Password,
                RowSchema = ConvertEventSchemaFromV1(jdbcOutputConf.RowSchema),
                TableName = jdbcOutputConf.TableName,
                UserName = jdbcOutputConf.UserName
            },
            V1.KafkaOutputConf kafkaOutputConf => new Actual.KafkaOutputConf
            {
                Broker = kafkaOutputConf.Broker,
                Parallelism = kafkaOutputConf.Parallelism,
                RowSchema = ConvertEventSchemaFromV1(kafkaOutputConf.RowSchema),
                Serializer = kafkaOutputConf.Serializer,
                Topic = kafkaOutputConf.Topic
            },
            _ => throw new ArgumentException($"Unsupported output configuration: {outputConf} was provided")
        };

    public static Actual.IOutputConf ConvertOutputConfFromV2(V2.IOutputConf outputConf)
        => outputConf switch
        {
            V2.JdbcOutputConf jdbcOutputConf => new Actual.JdbcOutputConf
            {
                BatchInterval = jdbcOutputConf.BatchInterval,
                DriverName = jdbcOutputConf.DriverName,
                JdbcUrl = jdbcOutputConf.JdbcUrl,
                Parallelism = jdbcOutputConf.Parallelism,
                Password = jdbcOutputConf.Password,
                RowSchema = ConvertEventSchemaFromV2(jdbcOutputConf.RowSchema),
                TableName = jdbcOutputConf.TableName,
                UserName = jdbcOutputConf.UserName
            },
            V2.KafkaOutputConf kafkaOutputConf => new Actual.KafkaOutputConf
            {
                Broker = kafkaOutputConf.Broker,
                Parallelism = kafkaOutputConf.Parallelism,
                RowSchema = ConvertEventSchemaFromV2(kafkaOutputConf.RowSchema),
                Serializer = kafkaOutputConf.Serializer,
                Topic = kafkaOutputConf.Topic
            },
            _ => throw new ArgumentException($"Unsupported output configuration: {outputConf} was provided")
        };

    public static Actual.IOutputConf ConvertOutputConfFromV3(V3.IOutputConf outputConf)
    {
        return outputConf;
    }


    public static Actual.EventSchema ConvertEventSchemaFromV1(V1.EventSchema eventSchema) => CreateEventSchema(
        new List<(string?, Actual.IEventSchemaValue)>()
        {
            (eventSchema.AppIdFieldVal.Item1, new Actual.IntegerEventSchemaValue { Type = "int32", Value = eventSchema.AppIdFieldVal.Item2 }),
            (eventSchema.FromTsField, new Actual.StringEventSchemaValue { Type = "timestamp", Value = "$IncidentStart" }),
            (eventSchema.PatternIdField, new Actual.StringEventSchemaValue { Type = "int32", Value = "$PatternID" }),
            (eventSchema.SubunitIdField, new Actual.StringEventSchemaValue { Type = "int32", Value = "$Subunit" }),
            (eventSchema.UnitIdField, new Actual.StringEventSchemaValue { Type = "int32", Value = "$Unit" }),
            (eventSchema.ToTsField, new Actual.StringEventSchemaValue { Type = "timestamp", Value = "$IncidentEnd" }),
        }
    );

    public static Actual.EventSchema ConvertEventSchemaFromV2(V2.EventSchema eventSchema) => CreateEventSchema(
        new List<(string?, Actual.IEventSchemaValue)>()
        {
            (eventSchema.AppIdFieldVal.Item1, new Actual.IntegerEventSchemaValue { Type = "int32", Value = eventSchema.AppIdFieldVal.Item2 }),
            (eventSchema.Context?.Field, new Actual.ObjectEventSchemaValue
            {
                Type = "object",
                Value = eventSchema.Context?.Data.Select(
                    kv => (kv.Key, new Actual.StringEventSchemaValue { Type = "string", Value = kv.Value })
                    ).ToDictionary(kv => kv.Item1, kv => kv.Item2 as Actual.IEventSchemaValue, null) ?? new Dictionary<string, Actual.IEventSchemaValue>()
            }),
            (eventSchema.FromTsField, new Actual.StringEventSchemaValue { Type = "timestamp", Value = "$IncidentStart" }),
            (eventSchema.IncidentIdField, new Actual.StringEventSchemaValue { Type = "string", Value = "$UUID" }),
            (eventSchema.PatternIdField, new Actual.StringEventSchemaValue { Type = "int32", Value = "$PatternID" }),
            (eventSchema.SubunitIdField, new Actual.StringEventSchemaValue { Type = "int32", Value = "$Subunit" }),
            (eventSchema.ToTsField, new Actual.StringEventSchemaValue { Type = "timestamp", Value = "$IncidentEnd" }),
            (eventSchema.UnitIdField, new Actual.StringEventSchemaValue { Type = "int32", Value = "$Unit" }),

        }
    );

    public static Actual.EventSchema ConvertEventSchemaFromV3(V3.EventSchema eventSchema) => eventSchema;


    public static Actual.EventSchema CreateEventSchema(List<(string?, Actual.IEventSchemaValue)> data)
    {
        var schema = new Actual.EventSchema();
        schema.Data = new Dictionary<string, Actual.IEventSchemaValue>();
        foreach (var (k, v) in data)
        {
            if (k != null) schema.Data[k] = v;
        }
        return schema;
    }
}