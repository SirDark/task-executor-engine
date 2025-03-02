using API.Models.Dto;
using API.Services.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;
using API.Options;
using Microsoft.Extensions.Options;
using API.Repositories;

namespace API.Services.HostedService;

public class UpdateTaskService : BackgroundService
{
    private IChannel? _channel;
    private readonly IRabbitMQService _rabbitMQService;
    private readonly IServiceScopeFactory _serviceFactory;
    private string _updateQueueName;
    public UpdateTaskService(IOptions<RabbitOptions> options,
        IRabbitMQService service,
        IServiceScopeFactory serviceFactory)
    {
        _rabbitMQService = service;
        _updateQueueName = options.Value.UpdateQueueName;
        _serviceFactory = serviceFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _rabbitMQService.GetChannelAsync(_updateQueueName);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                var taskMessage = JsonConvert.DeserializeObject<UpdateTaskDto>(messageJson);
                Console.WriteLine(taskMessage!.id);
                Console.WriteLine("exir code" + taskMessage!.exit_code);
                using (var scope = _serviceFactory.CreateScope())
                {
                    var repository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
                    var task = await repository.UpdateTask(taskMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
            
        };

        await _channel.BasicConsumeAsync(queue: _updateQueueName, autoAck: true, consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}
