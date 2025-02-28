using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace TaskExecutor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private IChannel? _channel;
        private string QueueName = "task_queue";
        //private readonly HttpClient _httpClient = new() { BaseAddress = new Uri("http://localhost:5000/") };

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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            if (_channel == null)
            {
                _connection = await _factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();
                await _channel.QueueDeclareAsync(queue: QueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);

            }
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(message);
                // Wait for 100 milliseconds
                await Task.Delay(10);

                Console.WriteLine("Waited 100ms before processing next message");
            };

            await _channel.BasicConsumeAsync(queue: QueueName, autoAck: true, consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
            /*var tasks = await _httpClient.GetFromJsonAsync<List<TaskItem>>("api/tasks/queued");
            if (tasks == null || tasks.Count == 0)
            {
                await Task.Delay(2000, stoppingToken);
                continue;
            }*/

            /*foreach (var engineTask in tasks)
            {
                _logger.LogInformation($"Executing Task {engineTask.Id}: {engineTask.Command}");
                await _httpClient.PutAsJsonAsync($"api/tasks/update/{engineTask.Id}", engineTask);

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{engineTask.Command}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false
                    }
                };

                process.Start();
                    
                process.WaitForExit();
                var exitCode = process.ExitCode;

                await _httpClient.PutAsJsonAsync($"api/tasks/update/{engineTask.Id}", engineTask);
                _logger.LogInformation($"Task {engineTask.Id} completed with exit code {exitCode}");
            }*/
        }

    }
}
public record TaskItem(Guid Id, string Command);