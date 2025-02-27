using API.Models.Dto;
using API.Models.Entities;
using API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/tasks")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly ILogger<TasksController> _logger;
    private readonly ITaskRepository _taskRepository;
    public TasksController(ILogger<TasksController> logger, ITaskRepository taskRepository)
    {
        _logger = logger;
        _taskRepository = taskRepository;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CommandDto command)//the task specified that I the request body should look like '{"command": "echo hello && sleep10"}' that is why I use the CommandDto in this case
    {
        EngineTask task = new EngineTask
        {
            id = Guid.NewGuid(),
            command = command.command,
            status = Status.queued,
            started_at = DateTime.UtcNow,
        };
        await _taskRepository.CreateAsync(task);
        return Ok();
    }
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var tasks = await _taskRepository.GetAllAsync();
        return Ok(tasks);
    }
}
