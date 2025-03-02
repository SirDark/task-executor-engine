using API.Models.Dto;
using API.Models.Entities;
using API.Repositories;
using API.Services.RabbitMQ;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/tasks")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly ILogger<TasksController> _logger;
    private readonly ITaskRepository _taskRepository;
    private readonly IRabbitMQService _rabbitMQService;
    public TasksController(ILogger<TasksController> logger, ITaskRepository taskRepository, IRabbitMQService rabbitMQService)
    {
        _logger = logger;
        _taskRepository = taskRepository;
        _rabbitMQService = rabbitMQService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CommandDto command)//the task specified that I the request body should look like '{"command": "echo hello && sleep10"}' that is why I use the CommandDto in this case
    {
        EngineTask task = new EngineTask
        {
            id = Guid.NewGuid(),
            command = command.command,
            status = Status.queued.ToString(),
            started_at = null,
        };
        await _taskRepository.CreateAsync(task);
        await _rabbitMQService.PublishTaskAsync(task.command, task.id);
        return Created();
    }
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var tasks = await _taskRepository.GetAllAsync();
        return Ok(tasks);
    }
}
