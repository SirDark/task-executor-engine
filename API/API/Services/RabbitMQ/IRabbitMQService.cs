using RabbitMQ.Client;

namespace API.Services.RabbitMQ;

public interface IRabbitMQService
{
    public Task PublishTaskAsync(string command, Guid taskId);
    public Task<IChannel> GetChannelAsync(string queueName);
}
