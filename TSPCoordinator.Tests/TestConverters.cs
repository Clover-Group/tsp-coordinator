using System.Collections.Generic;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TspCoordinator.Data.TspApi;
using V1 = TspCoordinator.Data.TspApi.V1;
using V2 = TspCoordinator.Data.TspApi.V2;
using V3 = TspCoordinator.Data.TspApi.V3;
using Dahomey.Json;

namespace TSPCoordinator.Tests;

[TestClass]
public class TestConverters
{
    private JsonSerializerOptions jsonOptions = new JsonSerializerOptions();

    private V1.Request v1Request = new V1.Request
    {
        Uuid = "35af70b5-b5b7-4458-bf08-f8f96779c8f5",
        Source = new V1.JdbcInputConf
        {
            Query = "SELECT * FROM test_table",
            JdbcUrl = "jdbc:clickhouse://127.0.0.1:8123/test",
            DriverName = "com.clickhouse.jdbc.ClickHouseDriver",
            SourceId = 42,
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
                PatternIdField = "pattern_id"
            }
        },
        Patterns = new List<V1.Pattern>
        {
            new V1.Pattern { Id = "123", SourceCode = "test_sensor1 > 0" }
        }
    };
    private V2.Request v2Request = new V2.Request
    {
        Uuid = "35af70b5-b5b7-4458-bf08-f8f96779c8f5",
        Source = new V2.JdbcInputConf
        {
            Query = "SELECT * FROM test_table",
            JdbcUrl = "jdbc:clickhouse://127.0.0.1:8123/test",
            DriverName = "com.clickhouse.jdbc.ClickHouseDriver",
            SourceId = 42,
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
    private V3.Request v3Request = new V3.Request
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

    public TestConverters() 
    {
        foreach (var c in TspCoordinator.Data.TspApi.JsonConverters.Converters)
        {
            jsonOptions.Converters.Add(c);
        }
        jsonOptions.SetupExtensions();
    }

    [TestMethod]
    public void TestConversionFromV1()
    {
        var result = Converters.ConvertRequestFromV1(v1Request);
        Assert.AreEqual(JsonSerializer.Serialize(v3Request, jsonOptions), JsonSerializer.Serialize(result, jsonOptions));
    }

    [TestMethod]
    public void TestConversionFromV2()
    {
        var result = Converters.ConvertRequestFromV2(v2Request);
        Assert.AreEqual(JsonSerializer.Serialize(v3Request, jsonOptions), JsonSerializer.Serialize(result, jsonOptions));
    }

    [TestMethod]
    public void TestConversionFromV3()
    {
        var result = Converters.ConvertRequestFromV3(v3Request);
        Assert.AreEqual(v3Request, result);
    }
}