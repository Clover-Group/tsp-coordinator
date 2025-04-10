using Microsoft.AspNetCore.Mvc;
using TspCoordinator.Data.Grammars;

namespace TspCoordinator.Controllers;


[ApiController]
[Route("api/check")]
public class CheckController(CheckService checkService) : Controller
{
    private readonly CheckService _checkService = checkService;

    [HttpPost("check")]
    public IActionResult Check(CheckRequest checkRequest) => Ok(_checkService.Check(checkRequest));
}
