
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


    /// <summary>
    ///  Requests list of all jobs and the information about them.
    /// </summary>
    /// <param name="show">
    /// Filter for the job list. Possible values are:
    /// "queued" - show only the job queue;
    /// "running" - show only the jobs which are running at the moment;
    /// "completed" - show only the completed jobs;
    /// "all" - show all jobs (queued, running and completed).
    /// </param>
    /// <returns>Job list</returns>
    /// <response code="200">Success</response>
    /// <response code="400">Invalid value for "show" parameter</response>
    [HttpGet("overview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    ///  Provides details about the given job.
    /// </summary>
    /// <param name="id">The ID of a job to retrieve</param>
    /// <returns>Details about the job</returns>
    /// <response code="200">Success</response>
    /// <response code="404">The specified job ID was not found.</response>
    [HttpGet("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Status(string id)
    {
        var jobs = await _jobService.GetAllJobsAsync();
        return jobs.Find(x => x.JobId == id) switch
        {
            null => NotFound($"Job with id {id} not found."),
            Job j => Ok(j),
        };
    }

    /// <summary>
    ///  Orders to stop the job (either dequeue (if queued) or request TSP to stop (if running)).
    /// </summary>
    /// <param name="id">The ID of a job to stop</param>
    /// <response code="200">Success</response>
    /// <response code="404">The specified job ID was not found (the job can be already completed).</response>
    [HttpGet("{id}/stop")]
    [HttpPost("{id}/stop")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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