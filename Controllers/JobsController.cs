
using Microsoft.AspNetCore.Mvc;
using TspCoordinator.Data;

[ApiController]
[Route("api/jobs")]
public class JobsController : Controller
{
    private JobService _jobService;

    public JobsController(JobService jobService)
    {
        _jobService = jobService;
    }


    [HttpGet("overview")]
    public async Task<IActionResult> Overview([FromQuery] string show = "all")
    {
        switch (show)
        {
            case "queued":
            case "running":
            case "completed":
            case "all":
                break;
            default:
                return BadRequest(
                    $"Invalid 'show' query parameter: '{show}'. Must be 'queued', 'running', 'completed' or 'all'."
                    );
        };

        IEnumerable<Job> jobs = show switch
        {
            "queued" => await _jobService.GetJobQueueAsync(),
            "running" => await _jobService.GetRunningJobsAsync(),
            "completed" => await _jobService.GetCompletedQueueAsync(),
            "all" => await _jobService.GetAllJobsAsync(),
            _ => new List<Job>() // unreachable
        };
        return Ok(jobs);
    }

    [HttpGet("{id}/status")]
    public async Task<IActionResult> Status(string id)
    {
        var jobs = await _jobService.GetAllJobsAsync();
        return jobs.Find(x => x.JobId == id) switch {
            null => NotFound($"Job with id {id} not found."),
            Job j => Ok(j),
        };
    }

    [HttpPost("{id}/stop")]
    public async Task<IActionResult> Stop(string id)
    {
        return await _jobService.StopJob(id) switch
        {
            JobStopResult.Dequeued => Ok($"Job {id} successfully dequeued."),
            JobStopResult.StopRequested => Ok($"Stop request for job {id} sent."),
            JobStopResult.NotFound => NotFound($"Job with {id} not queued or running (can be already completed)."),
            _ => StatusCode(500, "Something went wrong, invalid value for job stop status reported.")
        };
    }
}