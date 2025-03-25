using Microsoft.AspNetCore.Mvc;
using Semver;
using TspCoordinator.Data;

namespace TspCoordinator.Controllers;

public class TspRegisterInfo
{
    public string Version { get; set; } = "";
    public Guid? Uuid { get; set; } = null;

    public string? AdvertisedIp { get; set; } = null;

    public int? AdvertisedPort { get; set; } = null;

    public int? JobLimit { get; set; } = null;
}

public class TspUnregisterInfo
{
    public Guid Uuid { get; set; } = new();
}

public class JobStartedInfo
{
    public string JobId { get; set; } = "";
}

public record class JobCompletedInfo
{
    public string JobId { get; set; } = "";
    public bool Success { get; set; }

    public string? Error { get; set; }

    public bool? Fatal { get; set; }

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
        System.Net.IPAddress host = System.Net.IPAddress.Loopback;
        bool hostAdvertised = System.Net.IPAddress.TryParse(info.AdvertisedIp, out host);
        if (!hostAdvertised)
        {
            if (Request.HttpContext.Connection.RemoteIpAddress == null)
            {
                return BadRequest();
            }
            else
            {
                host = Request.HttpContext.Connection.RemoteIpAddress;
            }
        }
        TspInstance instance = new()
        {
            Uuid = info.Uuid ?? GetIDFromIP(Request.HttpContext.Connection.RemoteIpAddress),
            Host = host,
            Port = info.AdvertisedPort ?? 8080, //Request.HttpContext.Connection.RemotePort,
            IsHostAdvertised = hostAdvertised,
            IsPortAdvertised = info.AdvertisedPort.HasValue,
            Version = SemVersion.Parse(info.Version),
            TotalJobsLimit = info.JobLimit ?? 0,
            HealthCheckDate = DateTime.Now
        };
        if (_instancesService.AddInstance(instance))
        {
            return CreatedAtAction(nameof(Register), instance);
        }
        else
        {
            return StatusCode(StatusCodes.Status208AlreadyReported);
        }
    }

    /// <summary>
    /// Gets the list of TSP instances
    /// </summary>
    /// <returns>List of TSP instances</returns>
    /// <response code="200">Success</response>
    [HttpGet("instances")]
    public async Task<IActionResult> Instances()
    {
        return Ok(await _instancesService.GetInstancesAsync());
    }

    /// <summary>
    ///  Unregisters TSP instance.
    /// </summary>
    /// <param name="info">Information about TSP instance (instance ID)</param>
    /// <returns>Nothing</returns>
    /// <response code="204">TSP unregistration is successful.</response>
    /// <response code="404">No TSP instance with thid ID was found (it may be unregistered earlier).</response>
    [HttpPost("unregister")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Unregister([FromBody] TspUnregisterInfo info)
    {
        switch (_instancesService.FindById(info.Uuid))
        {
            case TspInstance instance:
                _instancesService.RemoveInstance(instance);
                return NoContent();
            case null:
                return NotFound();
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
    public async Task<IActionResult> JobCompleted([FromBody] JobCompletedInfo info)
    {
        //Console.WriteLine($"OnJobCompleted: {info}");
        await _jobService.OnJobCompleted(info);
        return Ok();
    }
    public static Guid GetIDFromIP(System.Net.IPAddress address)
    {
        /* 
            For older TSP versions (19.2.1 and below), compute ID from IP address
            in the following form:  	00545350-0000-0000-0000-0000xxxxxxxx,
            where '00545350' is a magic constant standing for 'TSP',
            and 'xxxxxxxx' are IPv4 address bytes.
            E.g. for IP 10.83.0.8 the ID will be 00545350-0000-0000-0000-00000a530008.
            Newer versions (19.2.2+) send unique IDs upon registration.
        */
        var guidBinaryData = new byte[16];
        guidBinaryData[3] = 0x00;
        guidBinaryData[2] = 0x54; // 'T'
        guidBinaryData[1] = 0x53; // 'S'
        guidBinaryData[0] = 0x50; // 'P'
        var addressBytes = address.MapToIPv4().GetAddressBytes();
        Array.Copy(addressBytes, 0, guidBinaryData, 12, 4);
        return new Guid(guidBinaryData);
    }
}
