using API.Models.Entities;

namespace API.Repositories;

public interface ITaskRepository
{
    public Task<List<EngineTask>?> GetAllAsync();
    public Task<EngineTask?> CreateAsync(EngineTask task);
}
