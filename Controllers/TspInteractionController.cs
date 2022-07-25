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

    [HttpPost("register")]
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

    [HttpPost("jobstarted")]
    public IActionResult JobStarted([FromBody] JobStartedInfo info)
    {
        _jobService.OnJobStarted(info);
        return Ok();
    }

    [HttpPost("jobcompleted")]
    public IActionResult JobCompleted([FromBody] JobCompletedInfo info)
    {
        _jobService.OnJobCompleted(info);
        return Ok();
    }
}
