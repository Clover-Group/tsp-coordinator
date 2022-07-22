using Microsoft.AspNetCore.Mvc;
using TspCoordinator.Data.TspApi.V2;
using TspCoordinator.Data.TspApi;
using TspCoordinator.Data;

namespace TspCoordinator.Controllers;

[ApiController]
[Route("api/v2")]
public class V2Controller : Controller
{
    private JobService _jobService;

    public V2Controller(JobService jobService)
    {
        _jobService = jobService;
    }

    [HttpPost("from-{source}/to-{sink}")]
    public IActionResult AddJob(string source, string sink, [FromBody] Request request)
    {
        if (source != "jdbc" && source != "kafka")
        {
            return BadRequest($"Source `{source}` not supported.");
        }
        if (sink != "jdbc" && sink != "kafka")
        {
            return BadRequest($"Sink `{sink}` not supported.");
        }
        var sourceOk = source switch
        {
            "jdbc" => request.Source is JdbcInputConf,
            "kafka" => request.Source is KafkaInputConf
        };
        var sinkOk = sink switch
        {
            "jdbc" => request.Sink is JdbcOutputConf,
            "kafka" => request.Sink is KafkaOutputConf
        };
        if (!(sourceOk && sinkOk))
        {
            return BadRequest("Endpoint does not match source/sink types.");
        }
        var actualRequest = Converters.ConvertRequestFromV2(request);
        var job = new Job
        {
            Request = actualRequest,
            JobId = actualRequest.Uuid
        };
        _jobService.EnqueueJob(job);
        return Ok (job);
    }
}
