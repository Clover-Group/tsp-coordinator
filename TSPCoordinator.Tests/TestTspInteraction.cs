using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Dahomey.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TspCoordinator.Controllers;

namespace TspCoordinator.Tests;
[TestClass]
public class TestTspInteraction
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
    public async Task TestTspRegister()
    {
        // try to register without advertised IP (should fail)
        {
            TspRegisterInfo info = new() { Version = "20.0.0", Uuid = Guid.NewGuid() };
            var client = _factory.CreateClient();
            var data = JsonSerializer.Serialize(info, _jsonOptions);
            var buffer = System.Text.Encoding.UTF8.GetBytes(data);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync("api/tspinteraction/register", byteContent);
            Console.WriteLine($"RESPONSE = {await response.Content.ReadAsStringAsync()}");
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            response = await client.PostAsync("api/tspinteraction/register", byteContent);
            Console.WriteLine($"RESPONSE = {await response.Content.ReadAsStringAsync()}");
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
        // try to register with advertised IP (should succeed)
        {
            TspRegisterInfo info = new() { Version = "20.0.0", AdvertisedIp = "127.0.0.1", AdvertisedPort = 8080, Uuid = Guid.NewGuid() };
            var client = _factory.CreateClient();
            var data = JsonSerializer.Serialize(info, _jsonOptions);
            var buffer = System.Text.Encoding.UTF8.GetBytes(data);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync("api/tspinteraction/register", byteContent);
            Console.WriteLine($"RESPONSE = {await response.Content.ReadAsStringAsync()}");
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            response = await client.PostAsync("api/tspinteraction/register", byteContent);
            Console.WriteLine($"RESPONSE = {await response.Content.ReadAsStringAsync()}");
            Assert.AreEqual(HttpStatusCode.AlreadyReported, response.StatusCode);
        }
    }
}