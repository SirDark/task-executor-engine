using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using TaskExecutor.Data;

namespace TaskExecutor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private IChannel? _taskQueueChannel;
        private IChannel? _updateQueueChannel;
        private string TaskQueueName = "task_queue";
        private string UpdateQueueName = "update_queue";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            _factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",
                UserName = "user",
                Password = "password"
            };
        }
        private async Task ProcessMessage(TaskMessage taskMessage)
        {
            UpdateMessage updateInProgress = new
            (
                Id: taskMessage.TaskId,
                started_at: DateTime.UtcNow.ToUniversalTime(),
                finished_at: null,
                status: Status.in_progress,
                exit_code: null,
                stderr: null,
                stdout: null
            );
            await PublishTaskAsync(updateInProgress);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{taskMessage.Command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            process.Start();

            string stderr = await process.StandardError.ReadToEndAsync();
            string stdout = await process.StandardOutput.ReadToEndAsync();

            process.WaitForExit();
            Console.WriteLine("exit code: " + process.ExitCode);
            UpdateMessage updateFinished = new(
                Id: taskMessage.TaskId,
                started_at: updateInProgress.started_at,
                finished_at: DateTime.UtcNow.ToUniversalTime(),
                status: Status.finished,
                exit_code: process.ExitCode,
                stderr: stderr,
                stdout: stdout
            );
            await PublishTaskAsync(updateFinished);

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            if (_taskQueueChannel == null)
            {
                _connection = await _factory.CreateConnectionAsync();
                _taskQueueChannel = await _connection.CreateChannelAsync();
                _updateQueueChannel = await _connection.CreateChannelAsync();
                await _taskQueueChannel.QueueDeclareAsync(queue: TaskQueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);
                await _updateQueueChannel.QueueDeclareAsync(queue: UpdateQueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);

            }
            var consumer = new AsyncEventingBasicConsumer(_taskQueueChannel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                var taskMessage = JsonConvert.DeserializeObject<TaskMessage>(messageJson);
                if (taskMessage != null)
                    await ProcessMessage(taskMessage);
            };

            await _taskQueueChannel.BasicConsumeAsync(queue: TaskQueueName, autoAck: true, consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        public async Task PublishTaskAsync(UpdateMessage message)
        {
            var messageBody = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message));
            await _updateQueueChannel!.BasicPublishAsync(exchange: String.Empty, routingKey: UpdateQueueName, body: messageBody);
        }

    }
}
