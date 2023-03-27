using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Dahomey.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using V1 = TspCoordinator.Data.TspApi.V1;
using V2 = TspCoordinator.Data.TspApi.V2;
using V3 = TspCoordinator.Data.TspApi.V3;

namespace TSPCoordinator.Tests;
[TestClass]
public class TestSendJobs
{
    private static WebApplicationFactory<Program> _factory;
    private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };


    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        _factory = new WebApplicationFactory<Program>();
        foreach (var c in TspCoordinator.Data.TspApi.JsonConverters.Converters)
        {
            _jsonOptions.Converters.Add(c);
        }
        _jsonOptions.SetupExtensions();
    }

    [TestMethod]
    public async Task TestSendV1Job()
    {
        var client = _factory.CreateClient();
        var serializedRequest = JsonSerializer.Serialize(StaticData.V1Request, _jsonOptions);
        var request = JsonSerializer.Deserialize(serializedRequest, typeof(V1.Request), _jsonOptions) as V1.Request;
        if (request == null) throw new ArgumentNullException("Something was wrong during (de)serialization of V1 request");
        request.Uuid = Guid.NewGuid().ToString();
        var data = JsonSerializer.Serialize(request, _jsonOptions);
        var buffer = System.Text.Encoding.UTF8.GetBytes(data);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await client.PostAsync("api/v1/from-jdbc/to-jdbc", byteContent);
        response.EnsureSuccessStatusCode();

    }

    [TestMethod]
    public async Task TestSendV2Job()
    {
        var client = _factory.CreateClient();
        var serializedRequest = JsonSerializer.Serialize(StaticData.V2Request, _jsonOptions);
        var request = JsonSerializer.Deserialize(serializedRequest, typeof(V2.Request), _jsonOptions) as V2.Request;
        if (request == null) throw new ArgumentNullException("Something was wrong during (de)serialization of V2 request");
        request.Uuid = Guid.NewGuid().ToString();
        request.Sink.RowSchema.UnitIdField = "unit_id";
        request.Sink.RowSchema.SubunitIdField = "subunit_id";
        request.Sink.RowSchema.IncidentIdField = "subunit_id";
        var data = JsonSerializer.Serialize(request, _jsonOptions);
        var buffer = System.Text.Encoding.UTF8.GetBytes(data);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await client.PostAsync("api/v2/from-jdbc/to-jdbc", byteContent);
        response.EnsureSuccessStatusCode();

    }

    [TestMethod]
    public async Task TestSendV3Job()
    {
        var client = _factory.CreateClient();
        //var serializedRequest = JsonSerializer.Serialize(StaticData.V3Request, _jsonOptions);
        //var request = JsonSerializer.Deserialize(serializedRequest, typeof(V3.Request), _jsonOptions) as V3.Request;
        //if (request == null) throw new ArgumentNullException("Something was wrong during (de)serialization of V3 request");
        //request.Uuid = Guid.NewGuid().ToString();
        var data = JsonSerializer.Serialize(StaticData.V3Request, _jsonOptions);
        var buffer = System.Text.Encoding.UTF8.GetBytes(data);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await client.PostAsync("api/v3/job/submit", byteContent);
        response.EnsureSuccessStatusCode();

    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _factory.Dispose();
    }
}