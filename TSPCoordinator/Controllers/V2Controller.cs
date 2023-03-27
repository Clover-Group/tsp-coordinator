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

    /// <summary>
    ///  Send version 2 job request (syntax supported by TSP version 0.16 and 17)
    /// </summary>
    /// <param name="source">Type of the source (supported values are "jdbc" and "kafka")</param>
    /// <param name="sink">Type of the sink (supported values are "jdbc" and "kafka")</param>
    /// <param name="request">The request being sent</param>
    /// <returns>Newly created request</returns>
    /// <response code="200">Success</response>
    /// <response code="400">Request is malformed (e.g. configuration does not match the source/sink type)</response>
    /// <response code="409">Job with given UUID already exists in the coordinator</response>
    [HttpPost("from-{source}/to-{sink}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult AddJob(string source, string sink, [FromBody] Request request)
    {
        if (_jobService.FindJobById(request.Uuid) != null)
        {
            return Conflict($"Job with UUID `{request.Uuid}` already exists, cannot assign it right now");
        }
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
        return Ok(job);
    }
}
