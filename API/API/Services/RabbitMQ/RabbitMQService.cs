using API.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace API.Services.RabbitMQ;

public class RabbitMQService : IRabbitMQService
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;
    private string _queueName;

    public RabbitMQService(IOptions<RabbitOptions> options)
    {
        _factory = new ConnectionFactory() { 
            HostName = options.Value.Host,
            UserName = options.Value.Username,
            Password = options.Value.Password,
        };
        _queueName = options.Value.TaskQueueName;
    }

    private async Task InitalizeConnection()
    {
        if (_channel == null)
        {
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.QueueDeclareAsync(queue: _queueName, durable: true, autoDelete: false, exclusive: false,arguments: null);
            
        }
    }

    public async Task PublishTaskAsync(string command, Guid taskId)
    {
        if (_channel == null)
        {
            await InitalizeConnection();
        }

        var taskMessage = new { TaskId = taskId, Command = command };
        var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(taskMessage));


        await _channel!.BasicPublishAsync(exchange: String.Empty, routingKey: _queueName, body: messageBody);
    }
    public async Task<IChannel> GetChannelAsync(string queueName)
    {
        var newConnection = await _factory.CreateConnectionAsync();
        var newChannel = await newConnection.CreateChannelAsync();
        await newChannel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        return newChannel;
    }
}
