namespace TspCoordinator.Data.TspApi;

using System.Text.Json;
using Actual = V3;

public static class Converters
{

    public static Actual.Request ConvertRequestFromV1(V1.Request request) => new Actual.Request
    {
        Uuid = request.Uuid,
        Source = ConvertInputConfFromV1(request.Source),
        Sinks = new List<Actual.IOutputConf> { ConvertOutputConfFromV1(request.Sink) },
        Patterns = request.Patterns.Select(p => ConvertPatternFromV1(p)).ToList(),
        Priority = 0
    };

    public static Actual.Request ConvertRequestFromV2(V2.Request request) => new Actual.Request
    {
        Uuid = request.Uuid,
        Source = ConvertInputConfFromV2(request.Source),
        Sinks = new List<Actual.IOutputConf> { ConvertOutputConfFromV2(request.Sink) },
        Patterns = request.Patterns.Select(p => ConvertPatternFromV2(p)).ToList(),
        Priority = request.Priority
    };

    public static Actual.Request ConvertRequestFromV3(V3.Request request) => request;

    public static Actual.Pattern ConvertPatternFromV1(V1.Pattern pattern) => new Actual.Pattern
    {
        Id = Int32.TryParse(pattern.Id, out int patid) ? patid : -1,
        SourceCode = pattern.SourceCode,
        Metadata = pattern.Payload,
    };

    public static Actual.Pattern ConvertPatternFromV2(V2.Pattern pattern) => new Actual.Pattern
    {
        Id = pattern.Id,
        SourceCode = pattern.SourceCode,
        Metadata = pattern.Payload,
    };

    public static Actual.Pattern ConvertPatternFromV3(V3.Pattern pattern) => pattern;

    public static Actual.ISourceDataTransformation ConvertSDTFromV1(V1.ISourceDataTransformation sdt)
        => sdt switch
        {
            V1.NarrowDataUnfolding ndu => new Actual.NarrowDataUnfolding
            {
                Config = JsonSerializer.Deserialize<Actual.NarrowDataUnfoldingConf>(JsonSerializer.Serialize(ndu.Config))
            },
            V1.WideDataFilling wdf => new Actual.WideDataFilling
            {
                Config = JsonSerializer.Deserialize<Actual.WideDataFillingConf>(JsonSerializer.Serialize(wdf.Config))
            }
        };

    public static Actual.ISourceDataTransformation ConvertSDTFromV2(V2.ISourceDataTransformation sdt)
        => sdt switch
        {
            V2.NarrowDataUnfolding ndu => new Actual.NarrowDataUnfolding
            {
                Config = JsonSerializer.Deserialize<Actual.NarrowDataUnfoldingConf>(JsonSerializer.Serialize(ndu.Config))
            },
            V2.WideDataFilling wdf => new Actual.WideDataFilling
            {
                Config = JsonSerializer.Deserialize<Actual.WideDataFillingConf>(JsonSerializer.Serialize(wdf.Config))
            }
        };

    public static Actual.ISourceDataTransformation ConvertSDTFromV3(V3.ISourceDataTransformation sdt) => sdt;

    public static Actual.IInputConf ConvertInputConfFromV1(V1.IInputConf inputConf)
        => inputConf switch
        {
            V1.JdbcInputConf jdbcInputConf =>
                JsonSerializer.Deserialize<Actual.JdbcInputConf>(JsonSerializer.Serialize(jdbcInputConf)),
            V1.KafkaInputConf kafkaInputConf =>
                JsonSerializer.Deserialize<Actual.KafkaInputConf>(JsonSerializer.Serialize(kafkaInputConf))
        };

    public static Actual.IInputConf ConvertInputConfFromV2(V2.IInputConf inputConf)
        => inputConf switch
        {
            V2.JdbcInputConf jdbcInputConf =>
                JsonSerializer.Deserialize<Actual.JdbcInputConf>(JsonSerializer.Serialize(jdbcInputConf)),
            V2.KafkaInputConf kafkaInputConf =>
                JsonSerializer.Deserialize<Actual.KafkaInputConf>(JsonSerializer.Serialize(kafkaInputConf))
        };

    public static Actual.IInputConf ConvertInputConfFromV3(V3.IInputConf inputConf) => inputConf;

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
            }
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
            }
        };

    public static Actual.IOutputConf ConvertOutputConfFromV3(V3.IOutputConf outputConf) => outputConf;


    public static Actual.EventSchema ConvertEventSchemaFromV1(V1.EventSchema eventSchema) => new Actual.EventSchema
    {
        Data = new Dictionary<string, Actual.IEventSchemaValue>
        {
            [eventSchema.AppIdFieldVal.Item1] = new Actual.IntegerEventSchemaValue { Type = "int32", Value = eventSchema.AppIdFieldVal.Item2 },
            [eventSchema.ContextField] = new Actual.StringEventSchemaValue { Type = "string", Value = "Context not supported yet" },
            [eventSchema.FromTsField] = new Actual.StringEventSchemaValue { Type = "timestamp", Value = "$IncidentStart" },
            [eventSchema.PatternIdField] = new Actual.StringEventSchemaValue { Type = "int32", Value = "$PatternID" },
            [eventSchema.ProcessingTsField] = new Actual.StringEventSchemaValue { Type = "timestamp", Value = "$ProcessingDate" },
            [eventSchema.SourceIdField] = new Actual.IntegerEventSchemaValue { Type = "int32", Value = -1 },
            [eventSchema.ToTsField] = new Actual.StringEventSchemaValue { Type = "timestamp", Value = "$IncidentEnd" },

        }
    };

    public static Actual.EventSchema ConvertEventSchemaFromV2(V2.EventSchema eventSchema) => new Actual.EventSchema
    {
        Data = new Dictionary<string, Actual.IEventSchemaValue>
        {
            [eventSchema.AppIdFieldVal.Item1] = new Actual.IntegerEventSchemaValue { Type = "int32", Value = eventSchema.AppIdFieldVal.Item2 },
            [eventSchema.Context.Field] = new Actual.ObjectEventSchemaValue
            {
                Type = "object",
                Value = eventSchema.Context.Data.Select(
                    kv => (kv.Key, new Actual.StringEventSchemaValue { Type = "string", Value = kv.Value })
                    ).ToDictionary(kv => kv.Item1, kv => kv.Item2 as Actual.IEventSchemaValue, null)
            },
            [eventSchema.FromTsField] = new Actual.StringEventSchemaValue { Type = "timestamp", Value = "$IncidentStart" },
            [eventSchema.IncidentIdField] = new Actual.StringEventSchemaValue { Type = "timestamp", Value = "$UUID" },
            [eventSchema.PatternIdField] = new Actual.StringEventSchemaValue { Type = "int32", Value = "$PatternID" },
            [eventSchema.SubunitIdField] = new Actual.StringEventSchemaValue { Type = "int32", Value = "$Subunit" },
            [eventSchema.ToTsField] = new Actual.StringEventSchemaValue { Type = "timestamp", Value = "$IncidentEnd" },
            [eventSchema.UnitIdField] = new Actual.StringEventSchemaValue { Type = "int32", Value = "$Unit" },

        }
    };

    public static Actual.EventSchema ConvertEventSchemaFromV3(V3.EventSchema eventSchema) => eventSchema;

}