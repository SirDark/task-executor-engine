using API.Data;
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
}
