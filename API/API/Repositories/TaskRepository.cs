using API.Data;
using API.Models.Dto;
using API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ILogger<TaskRepository> _logger;
    private readonly AppDbContext _appDbContext;
    public TaskRepository(AppDbContext appDbContext, ILogger<TaskRepository> logger)
    {
        _logger = logger;
        _appDbContext = appDbContext;
    }
    public async Task<EngineTask?> CreateAsync(EngineTask task)
    {
        var created = await _appDbContext.AddAsync(task);
        await _appDbContext.SaveChangesAsync();
        return created.Entity;
    }

    public async Task<List<EngineTask>?> GetAllAsync()
    {
        return await _appDbContext.Tasks.ToListAsync();
    }

    public async Task<EngineTask?> GetTaskAsync(Guid id)
    {
        return await _appDbContext.Tasks.FirstOrDefaultAsync(x => x.id == id);
    }

    public async Task<EngineTask?> UpdateTask(UpdateTaskDto updateTask)
    {
        var engineTask = await _appDbContext.Tasks.FirstOrDefaultAsync(x => x.id == updateTask.id);
        if (engineTask == null) { 
            return null;
        }
        engineTask.status = updateTask.status.ToString();
        engineTask.exitcode = updateTask.exit_code;
        engineTask.stdout = updateTask.stdout;
        engineTask.stderr = updateTask.stderr;
        engineTask.started_at = updateTask.started_at;
        engineTask.finished_at  = updateTask.finished_at;
        await _appDbContext.SaveChangesAsync();
        return engineTask;
    }

}
