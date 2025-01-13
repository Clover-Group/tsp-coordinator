using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Dahomey.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TspCoordinator.Data;
using V1 = TspCoordinator.Data.TspApi.V1;
using V2 = TspCoordinator.Data.TspApi.V2;
using V3 = TspCoordinator.Data.TspApi.V3;

namespace TspCoordinator.Tests;
[TestClass]
public class TestSendJobs
{
    private static WebApplicationFactory<Program> _factory = default!;
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
        var serializedRequest = JsonSerializer.Serialize(StaticData.V3Request, _jsonOptions);
        var request = JsonSerializer.Deserialize(serializedRequest, typeof(V3.Request), _jsonOptions) as V3.Request;
        if (request == null) throw new ArgumentNullException("Something was wrong during (de)serialization of V3 request");
        request.Uuid = Guid.NewGuid().ToString();
        var data = JsonSerializer.Serialize(request, _jsonOptions);
        var buffer = System.Text.Encoding.UTF8.GetBytes(data);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await client.PostAsync("api/v3/job/submit", byteContent);
        response.EnsureSuccessStatusCode();

    }

    [TestMethod]
    public async Task TestSendMultipleJobs()
    {
        var client = _factory.CreateClient();
        var serializedRequest = JsonSerializer.Serialize(StaticData.V3Request, _jsonOptions);
        var guid10 = "10";
        var guid20 = "20";
        // Send a job with priority 10
        {
            var request = JsonSerializer.Deserialize(serializedRequest, typeof(V3.Request), _jsonOptions) as V3.Request;
            if (request == null) throw new ArgumentNullException("Something was wrong during (de)serialization of V3 request");
            request.Uuid = guid10;
            request.Priority = 10;
            var data = JsonSerializer.Serialize(request, _jsonOptions);
            var buffer = System.Text.Encoding.UTF8.GetBytes(data);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync("api/v3/job/submit", byteContent);
            response.EnsureSuccessStatusCode();
        }
        // Send a job with priority 20
        {
            var request = JsonSerializer.Deserialize(serializedRequest, typeof(V3.Request), _jsonOptions) as V3.Request;
            if (request == null) throw new ArgumentNullException("Something was wrong during (de)serialization of V3 request");
            request.Uuid = guid20;
            request.Priority = 20;
            var data = JsonSerializer.Serialize(request, _jsonOptions);
            var buffer = System.Text.Encoding.UTF8.GetBytes(data);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync("api/v3/job/submit", byteContent);
            response.EnsureSuccessStatusCode();
        }
        // Check for the right priority
        {
            var response = await client.GetAsync("api/jobs/overview?show=all");

            response.EnsureSuccessStatusCode();
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

            var jobs = await response.Content.ReadFromJsonAsync<List<Job>>(_jsonOptions);
            Assert.IsNotNull(jobs);
            // Count is 5 due to the previous 3 jobs sent before
            Assert.AreEqual(5, jobs.Count);
            Assert.AreEqual("20", jobs[0].JobId);
            Assert.AreEqual("10", jobs[1].JobId);
        }
        // Send conflicting job (should return 409)
        {
            var request = JsonSerializer.Deserialize(serializedRequest, typeof(V3.Request), _jsonOptions) as V3.Request;
            if (request == null) throw new ArgumentNullException("Something was wrong during (de)serialization of V3 request");
            request.Uuid = guid20;
            var data = JsonSerializer.Serialize(request, _jsonOptions);
            var buffer = System.Text.Encoding.UTF8.GetBytes(data);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync("api/v3/job/submit", byteContent);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.Conflict);
        }
    }



    [ClassCleanup]
    public static void ClassCleanup()
    {
        _factory.Dispose();
    }
}