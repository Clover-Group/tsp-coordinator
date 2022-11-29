using Microsoft.AspNetCore.Mvc;
using TspCoordinator.Data.TspApi.V3;
using TspCoordinator.Data.TspApi;
using TspCoordinator.Data;

namespace TspCoordinator.Controllers;

[ApiController]
[Route("api/v3")]
public class V3Controller : Controller
{
    private JobService _jobService;

    public V3Controller(JobService jobService)
    {
        _jobService = jobService;
    }

    /// <summary>
    ///  Send version 3 job request (syntax supported by TSP version 18 and 19)
    /// </summary>
    /// <param name="request">The request being sent</param>
    /// <returns>Newly created request</returns>
    /// <response code="200">Success</response>
    /// <response code="400">Request is malformed (e.g. configuration does not match the source/sink type)</response>
    /// <response code="409">Job with given UUID already exists in the coordinator</response>
    [HttpPost("job/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult AddJob([FromBody] Request request)
    {
        if (_jobService.FindJobById(request.Uuid) != null)
        {
            return Conflict($"Job with UUID `{request.Uuid}` already exists, cannot assign it right now");
        }
        var actualRequest = Converters.ConvertRequestFromV3(request);
        var job = new Job
        {
            Request = actualRequest,
            JobId = actualRequest.Uuid
        };
        _jobService.EnqueueJob(job);
        return Ok (job);
    }
}
