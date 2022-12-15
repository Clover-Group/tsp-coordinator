using System.Collections.Generic;
using V1 = TspCoordinator.Data.TspApi.V1;
using V2 = TspCoordinator.Data.TspApi.V2;
using V3 = TspCoordinator.Data.TspApi.V3;

namespace TSPCoordinator.Tests;

public static class StaticData
{
    public static V1.Request V1Request = new V1.Request
    {
        Uuid = "35af70b5-b5b7-4458-bf08-f8f96779c8f5",
        Source = new V1.JdbcInputConf
        {
            Query = "SELECT * FROM test_table",
            JdbcUrl = "jdbc:clickhouse://127.0.0.1:8123/test",
            DriverName = "com.clickhouse.jdbc.ClickHouseDriver",
            SourceId = 42,
            DatetimeField = "dt",
            PartitionFields = new List<string> { "engine_id" },
            DataTransformation = new V1.NarrowDataUnfolding 
            {
                Config = new V1.NarrowDataUnfoldingConf
                {
                    //DefaultTimeout = 3600000,
                    FieldsTimeoutsMs = new Dictionary<string, long>
                    {
                        ["sensor1"] = 5000,
                        ["sensor2"] = 10000,
                        ["sensor3"] = 20000
                    }
                }
            }
            
        },
        Sink = new V1.JdbcOutputConf
        {
            JdbcUrl = "jdbc:clickhouse://127.0.0.1:8123/test",
            TableName = "sink",
            DriverName = "com.clickhouse.jdbc.ClickHouseDriver",
            RowSchema = new V1.EventSchema
            {
                FromTsField = "from_ts",
                ToTsField = "to_ts",
                AppIdFieldVal = ("app_id", 13),
                PatternIdField = "pattern_id",
                
            }
        },
        Patterns = new List<V1.Pattern>
        {
            new V1.Pattern { Id = "123", SourceCode = "test_sensor1 > 0" }
        }
    };
    public static V2.Request V2Request = new V2.Request
    {
        Uuid = "35af70b5-b5b7-4458-bf08-f8f96779c8f5",
        Source = new V2.JdbcInputConf
        {
            Query = "SELECT * FROM test_table",
            JdbcUrl = "jdbc:clickhouse://127.0.0.1:8123/test",
            DriverName = "com.clickhouse.jdbc.ClickHouseDriver",
            SourceId = 42,
            DatetimeField = "dt",
            PartitionFields = new List<string> { "engine_id" },
            DataTransformation = new V2.NarrowDataUnfolding
            {
                Config = new V2.NarrowDataUnfoldingConf
                {
                    //DefaultTimeout = 3600000,
                    FieldsTimeoutsMs = new Dictionary<string, long>
                    {
                        ["sensor1"] = 5000,
                        ["sensor2"] = 10000,
                        ["sensor3"] = 20000
                    }
                }
            }
        },
        Sink = new V2.JdbcOutputConf
        {
            JdbcUrl = "jdbc:clickhouse://127.0.0.1:8123/test",
            TableName = "sink",
            DriverName = "com.clickhouse.jdbc.ClickHouseDriver",
            RowSchema = new V2.EventSchema
            {
                FromTsField = "from_ts",
                ToTsField = "to_ts",
                AppIdFieldVal = ("app_id", 13),
                PatternIdField = "pattern_id"
            }
        },
        Patterns = new List<V2.Pattern>
        {
            new V2.Pattern { Id = 123, SourceCode = "test_sensor1 > 0" }
        }
    };
    public static V3.Request V3Request = new V3.Request
    {
        Uuid = "35af70b5-b5b7-4458-bf08-f8f96779c8f5",
        Source = new V3.SourceWithType
        {
            Type = "jdbc",
            Config = new V3.JdbcInputConf
            {
                Query = "SELECT * FROM test_table",
                JdbcUrl = "jdbc:clickhouse://127.0.0.1:8123/test",
                DriverName = "com.clickhouse.jdbc.ClickHouseDriver",
                SourceId = 42,
                DatetimeField = "dt",
                PartitionFields = new List<string> { "engine_id" },
                DataTransformation = new V3.NarrowDataUnfolding
                {
                    Config = new V3.NarrowDataUnfoldingConf
                    {
                        //DefaultTimeout = 3600000,
                        FieldsTimeoutsMs = new Dictionary<string, long>
                        {
                            ["sensor1"] = 5000,
                            ["sensor2"] = 10000,
                            ["sensor3"] = 20000
                        }
                    }
                }
            }
        },
        Sinks = new List<V3.SinkWithType>
        {
            new V3.SinkWithType
            {
                Type = "jdbc",
                Config = new V3.JdbcOutputConf
                {
                    JdbcUrl = "jdbc:clickhouse://127.0.0.1:8123/test",
                    TableName = "sink",
                    DriverName = "com.clickhouse.jdbc.ClickHouseDriver",
                    RowSchema = new V3.EventSchema
                    {
                        Data = new Dictionary<string, V3.IEventSchemaValue>
                        {
                            ["app_id"] = new V3.IntegerEventSchemaValue { Type = "int32", Value = 13 },
                            ["from_ts"] = new V3.StringEventSchemaValue { Type = "timestamp", Value = "$IncidentStart" },
                            ["pattern_id"] = new V3.StringEventSchemaValue { Type = "int32", Value = "$PatternID" },
                            ["to_ts"] = new V3.StringEventSchemaValue { Type = "timestamp", Value = "$IncidentEnd" }
                        }
                    }
                },
            }
        },
        Patterns = new List<V3.Pattern>
        {
            new V3.Pattern { Id = 123, SourceCode = "test_sensor1 > 0" }
        }
    };

    public static V1.Request V1RequestKafka = new V1.Request
    {
        Uuid = "35af70b5-b5b7-4458-bf08-f8f96779c8f5",
        Source = new V1.KafkaInputConf
        {
            Brokers = "127.0.0.1:9092",
            Topic = "test_topic",
            SourceId = 42,
            DatetimeField = "dt",
            PartitionFields = new List<string> { "engine_id" },
            DataTransformation = new V1.WideDataFilling
            {
                Config = new V1.WideDataFillingConf
                {
                    //DefaultTimeout = 3600000,
                    FieldsTimeoutsMs = new Dictionary<string, long>
                    {
                        ["sensor1"] = 5000,
                        ["sensor2"] = 10000,
                        ["sensor3"] = 20000
                    }
                }
            }

        },
        Sink = new V1.KafkaOutputConf
        {
            Broker = "127.0.0.1:9092",
            Topic = "test_sink",
            RowSchema = new V1.EventSchema
            {
                FromTsField = "from_ts",
                ToTsField = "to_ts",
                AppIdFieldVal = ("app_id", 13),
                PatternIdField = "pattern_id",

            }
        },
        Patterns = new List<V1.Pattern>
        {
            new V1.Pattern { Id = "123", SourceCode = "test_sensor1 > 0" }
        }
    };
    public static V2.Request V2RequestKafka = new V2.Request
    {
        Uuid = "35af70b5-b5b7-4458-bf08-f8f96779c8f5",
        Source = new V2.KafkaInputConf
        {
            Brokers = "127.0.0.1:9092",
            Topic = "test_topic",
            SourceId = 42,
            DatetimeField = "dt",
            PartitionFields = new List<string> { "engine_id" },
            DataTransformation = new V2.WideDataFilling
            {
                Config = new V2.WideDataFillingConf
                {
                    //DefaultTimeout = 3600000,
                    FieldsTimeoutsMs = new Dictionary<string, long>
                    {
                        ["sensor1"] = 5000,
                        ["sensor2"] = 10000,
                        ["sensor3"] = 20000
                    }
                }
            }
        },
        Sink = new V2.KafkaOutputConf
        {
            Broker = "127.0.0.1:9092",
            Topic = "test_sink",
            RowSchema = new V2.EventSchema
            {
                FromTsField = "from_ts",
                ToTsField = "to_ts",
                AppIdFieldVal = ("app_id", 13),
                PatternIdField = "pattern_id"
            }
        },
        Patterns = new List<V2.Pattern>
        {
            new V2.Pattern { Id = 123, SourceCode = "test_sensor1 > 0" }
        }
    };
    public static V3.Request V3RequestKafka = new V3.Request
    {
        Uuid = "35af70b5-b5b7-4458-bf08-f8f96779c8f5",
        Source = new V3.SourceWithType
        {
            Type = "kafka",
            Config = new V3.KafkaInputConf
            {
                Brokers = "127.0.0.1:9092",
                Topic = "test_topic",
                SourceId = 42,
                DatetimeField = "dt",
                PartitionFields = new List<string> { "engine_id" },
                DataTransformation = new V3.WideDataFilling
                {
                    Config = new V3.WideDataFillingConf
                    {
                        //DefaultTimeout = 3600000,
                        FieldsTimeoutsMs = new Dictionary<string, long>
                        {
                            ["sensor1"] = 5000,
                            ["sensor2"] = 10000,
                            ["sensor3"] = 20000
                        }
                    }
                }
            }
        },
        Sinks = new List<V3.SinkWithType>
        {
            new V3.SinkWithType
            {
                Type = "kafka",
                Config = new V3.KafkaOutputConf
                {
                    Broker = "127.0.0.1:9092",
                    Topic = "test_sink",
                    RowSchema = new V3.EventSchema
                    {
                        Data = new Dictionary<string, V3.IEventSchemaValue>
                        {
                            ["app_id"] = new V3.IntegerEventSchemaValue { Type = "int32", Value = 13 },
                            ["from_ts"] = new V3.StringEventSchemaValue { Type = "timestamp", Value = "$IncidentStart" },
                            ["pattern_id"] = new V3.StringEventSchemaValue { Type = "int32", Value = "$PatternID" },
                            ["to_ts"] = new V3.StringEventSchemaValue { Type = "timestamp", Value = "$IncidentEnd" }
                        }
                    }
                },
            }
        },
        Patterns = new List<V3.Pattern>
        {
            new V3.Pattern { Id = 123, SourceCode = "test_sensor1 > 0" }
        }
    };
}