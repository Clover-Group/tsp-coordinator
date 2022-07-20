using Microsoft.AspNetCore.Mvc;
using TSPCoordinator.Data;

namespace TSPCoordinator.Controllers;

public class TSPRegisterInfo
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
}

[ApiController]
[Route("api/[controller]")]
public class TSPInteractionController : ControllerBase
{
    TSPInstancesService _instancesService;
    JobService _jobService;

    public TSPInteractionController(TSPInstancesService instancesService, JobService jobService)
    {
        _instancesService = instancesService;
        _jobService = jobService;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] TSPRegisterInfo info)
    {
        TSPInstance instance = new TSPInstance 
        {
            Host = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
            Port = 8080,//Request.HttpContext.Connection.RemotePort,
            Version = info.Version,
            HealthCheckDate = DateTime.Now
        };
        _instancesService.AddInstance(instance);
        return CreatedAtAction(nameof(Register), instance);
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
