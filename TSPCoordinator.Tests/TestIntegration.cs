using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dahomey.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TspCoordinator.Data;

namespace TspCoordinator.Tests;
[TestClass]
public class TestIntegration
{
    private static WebApplicationFactory<Program> _factory = default!;
    private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static IContainer _container;

    const ushort TSP_PORT = 8080;
    const ushort TSP_MAPPED_PORT = 48080;


    [ClassInitialize]
    public static async Task ClassInit(TestContext testContext)
    {
        await TestcontainersSettings.ExposeHostPortsAsync(5000)
            .ConfigureAwait(false);

        _factory = new WebApplicationFactory<Program>();
        foreach (var c in Data.TspApi.JsonConverters.Converters)
        {
            _jsonOptions.Converters.Add(c);
        }
        _jsonOptions.SetupExtensions();

        _container = new ContainerBuilder()
        .WithImage("clovergrp/tsp:20.0.0-rc11")
        .WithPortBinding(TSP_MAPPED_PORT, TSP_PORT)
        .WithEnvironment(new Dictionary<string, string>()
        {
            ["COORDINATOR_ENABLED"] = "true",
            ["COORDINATOR_HOST"] = "host.testcontainers.internal",
            ["COORDINATOR_PORT"] = "5000"
        })
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(TSP_PORT).ForPath("metainfo/getVersion")))
        .WithCleanUp(true)
        .Build();
        await _container.StartAsync().ConfigureAwait(false);
        // for TSP registration
        Thread.Sleep(10000);
    }

    [TestMethod]
    public async Task TestTspInstanсeRegistered()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/tspinteraction/instances");
        response.EnsureSuccessStatusCode();
        var instances = await response.Content.ReadFromJsonAsync<TspInstance[]>(_jsonOptions);
        Assert.IsNotNull(instances);
        //Assert.AreEqual(1, instances.Length);
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        await _container.StopAsync().ConfigureAwait(false);
    }
}