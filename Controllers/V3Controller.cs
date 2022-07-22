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

    [HttpPost("job/submit")]
    public IActionResult AddJob([FromBody] Request request)
    {
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
