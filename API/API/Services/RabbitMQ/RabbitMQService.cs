using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace API.Services.RabbitMQ;

public class RabbitMQService : IRabbitMQService
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string QueueName = "task_queue";

    public RabbitMQService()
    {
        _factory = new ConnectionFactory() { 
            HostName = "rabbitmq",
            UserName = "user",
            Password = "password"
        };
    }

    private async Task InitalizeConnection()
    {
        if (_channel == null)
        {
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.QueueDeclareAsync(queue: QueueName, durable: true, autoDelete: false, exclusive: false,arguments: null);
            
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


        await _channel!.BasicPublishAsync(exchange: String.Empty, routingKey: QueueName, body: messageBody);
    }
}
