using API.Models.Entities;
using API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
    public async Task<IActionResult> Post([FromBody] EngineTask task)
    {
        await _taskRepository.CreateAsync(task);
        return Ok();
    }
    [HttpGet]
    public async Task<IActionResult> Get() {
        var tasks = await _taskRepository.GetAllAsync();
        return Ok(tasks);
    }
}
