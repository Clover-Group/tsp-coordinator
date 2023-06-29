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

namespace TSPCoordinator.Tests;
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
        TspRegisterInfo info = new TspRegisterInfo { Version = "19.0.1" };
        var client = _factory.CreateClient();
        var data = JsonSerializer.Serialize(info, _jsonOptions);
        var buffer = System.Text.Encoding.UTF8.GetBytes(data);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await client.PostAsync("api/tspinteraction/register", byteContent);
        Console.WriteLine($"RESPONSE = {await response.Content.ReadAsStringAsync()}");
        //Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        response = await client.PostAsync("api/tspinteraction/register", byteContent);
        Console.WriteLine($"RESPONSE = {await response.Content.ReadAsStringAsync()}");
        //Assert.AreEqual(HttpStatusCode.AlreadyReported, response.StatusCode);
    }
}