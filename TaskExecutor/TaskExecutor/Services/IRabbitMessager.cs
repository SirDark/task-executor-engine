using RabbitMQ.Client;
using TaskExecutor.Model;

namespace TaskExecutor.Services;

public interface IRabbitMessager
{
    public Task InitMessager();
    public IChannel? GetTaskChannel();
    public Task PublishUpdate(UpdateMessage message);
    public string GetTaskQueueName();
}
