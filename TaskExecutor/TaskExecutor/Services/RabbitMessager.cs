using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using TaskExecutor.Model;
using TaskExecutor.Options;

namespace TaskExecutor.Services;

public class RabbitMessager : IRabbitMessager
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _taskQueueChannel;
    private IChannel? _updateQueueChannel;
    private string _taskQueueName;
    private string _updateQueueName;
    public RabbitMessager(IOptions<RabbitOptions> options)
    {
        _factory = new ConnectionFactory()
        {
            HostName = options.Value.Host,
            UserName = options.Value.Username,
            Password = options.Value.Password,
        };
        _taskQueueName = options.Value.TaskQueueName;
        _updateQueueName = options.Value.UpdateQueueName;
    }
    public async Task InitMessager()
    {
        if (_taskQueueChannel == null)
        {
            _connection = await _factory.CreateConnectionAsync();
            _taskQueueChannel = await _connection.CreateChannelAsync();
            _updateQueueChannel = await _connection.CreateChannelAsync();
            await _taskQueueChannel.QueueDeclareAsync(queue: _taskQueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);
            await _updateQueueChannel.QueueDeclareAsync(queue: _updateQueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);

        }
    }
    public IChannel? GetTaskChannel()
    {
        return _taskQueueChannel;
    }
    public string GetTaskQueueName()
    {
        return _taskQueueName;
    }
    public async Task PublishUpdate(UpdateMessage message)
    {
        var messageBody = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message));
        await _updateQueueChannel!.BasicPublishAsync(exchange: String.Empty, routingKey: _updateQueueName, body: messageBody);
    }
}
