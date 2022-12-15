using System.Collections.Generic;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TspCoordinator.Data.TspApi;
using Dahomey.Json;

namespace TSPCoordinator.Tests;

[TestClass]
public class TestConverters
{
    private JsonSerializerOptions jsonOptions = new JsonSerializerOptions();

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
        var result = Converters.ConvertRequestFromV1(StaticData.V1Request);
        Assert.AreEqual(JsonSerializer.Serialize(StaticData.V3Request, jsonOptions), JsonSerializer.Serialize(result, jsonOptions));
    }

    [TestMethod]
    public void TestConversionFromV2()
    {
        var result = Converters.ConvertRequestFromV2(StaticData.V2Request);
        Assert.AreEqual(JsonSerializer.Serialize(StaticData.V3Request, jsonOptions), JsonSerializer.Serialize(result, jsonOptions));
    }

    [TestMethod]
    public void TestConversionFromV3()
    {
        var result = Converters.ConvertRequestFromV3(StaticData.V3Request);
        Assert.AreEqual(JsonSerializer.Serialize(StaticData.V3Request, jsonOptions), JsonSerializer.Serialize(result, jsonOptions));
    }

    [TestMethod]
    public void TestConversionFromV1Kafka()
    {
        var result = Converters.ConvertRequestFromV1(StaticData.V1RequestKafka);
        Assert.AreEqual(JsonSerializer.Serialize(StaticData.V3RequestKafka, jsonOptions), JsonSerializer.Serialize(result, jsonOptions));
    }

    [TestMethod]
    public void TestConversionFromV2Kafka()
    {
        var result = Converters.ConvertRequestFromV2(StaticData.V2RequestKafka);
        Assert.AreEqual(JsonSerializer.Serialize(StaticData.V3RequestKafka, jsonOptions), JsonSerializer.Serialize(result, jsonOptions));
    }

    [TestMethod]
    public void TestConversionFromV3Kafka()
    {
        var result = Converters.ConvertRequestFromV3(StaticData.V3RequestKafka);
        Assert.AreEqual(JsonSerializer.Serialize(StaticData.V3RequestKafka, jsonOptions), JsonSerializer.Serialize(result, jsonOptions));
    }
}