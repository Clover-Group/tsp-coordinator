using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSPCoordinator.Tests;
[TestClass]
public class TestJobsController
{
    private static WebApplicationFactory<Program> _factory;

    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        _factory = new WebApplicationFactory<Program>();
    }

    [TestMethod]
    public async Task TestOverview()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("api/jobs/overview?show=all");

        response.EnsureSuccessStatusCode();
        Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var json = await response.Content.ReadAsStringAsync();
        Assert.AreEqual("[]", json);
    }

    [TestMethod]
    public async Task TestSingleNonexistentJob()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("api/jobs/42/status");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        response = await client.PostAsync("api/jobs/42/stop", null);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _factory.Dispose();
    }
}