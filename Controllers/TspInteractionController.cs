using Microsoft.AspNetCore.Mvc;
using TspCoordinator.Data;

namespace TspCoordinator.Controllers;

public class TspRegisterInfo
{
    public string Version { get; set; }
}

public class JobStartedInfo
{
    public string JobId { get; set; }
}

public class JobCompletedInfo
{
    public string JobId { get; set; }
    public bool Success { get; set; }

    public string? Error { get; set; }

    public long? RowsRead { get; set; }
    public long? RowsWritten { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class TspInteractionController : ControllerBase
{
    TspInstancesService _instancesService;
    JobService _jobService;

    public TspInteractionController(TspInstancesService instancesService, JobService jobService)
    {
        _instancesService = instancesService;
        _jobService = jobService;
    }

    /// <summary>
    ///  Registers TSP instance.
    /// </summary>
    /// <param name="info">Information about TSP instance (currently: version only)</param>
    /// <returns>A newly created TSP instance.</returns>
    /// <response code="201">TSP registration is successful.</response>
    /// <response code="208">TSP instance was already registered.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status208AlreadyReported)]
    public IActionResult Register([FromBody] TspRegisterInfo info)
    {
        if (Request.HttpContext.Connection.RemoteIpAddress == null)
        {
            return BadRequest();
        }
        TspInstance instance = new TspInstance 
        {
            Host = Request.HttpContext.Connection.RemoteIpAddress,
            Port = 8080,//Request.HttpContext.Connection.RemotePort,
            Version = info.Version,
            HealthCheckDate = DateTime.Now
        };
        if(_instancesService.AddInstance(instance))
        {
            return CreatedAtAction(nameof(Register), instance);
        }
        else
        {
            return StatusCode(StatusCodes.Status208AlreadyReported);
        }
    }

    /// <summary>
    ///  Notifies the coordinator about the successful start of a job.
    /// </summary>
    /// <response code="200">Coordinator is successfully notified</response>
    [HttpPost("jobstarted")]
    public IActionResult JobStarted([FromBody] JobStartedInfo info)
    {
        _jobService.OnJobStarted(info);
        return Ok();
    }

    /// <summary>
    ///  Notifies the coordinator about the completion of a job.
    /// </summary>
    /// <response code="200">Coordinator is successfully notified</response>
    [HttpPost("jobcompleted")]
    public IActionResult JobCompleted([FromBody] JobCompletedInfo info)
    {
        _jobService.OnJobCompleted(info);
        return Ok();
    }
}
