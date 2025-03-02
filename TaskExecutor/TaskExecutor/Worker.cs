using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using TaskExecutor.Model;
using TaskExecutor.Services;

namespace TaskExecutor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IRabbitMessager _rabbitMessager;

        public Worker(ILogger<Worker> logger, IRabbitMessager rabbitMessager)
        {
            _logger = logger;
            _rabbitMessager = rabbitMessager;
        }
        private async Task ProcessMessage(TaskMessage taskMessage)
        {
            UpdateMessage updateInProgress = new UpdateMessage
            {
                Id = taskMessage.TaskId,
                started_at = DateTime.UtcNow.ToUniversalTime(),
                finished_at = null,
                status = Status.in_progress,
                exit_code = null,
                stderr = null,
                stdout = null
            };
            await _rabbitMessager.PublishUpdate(updateInProgress);

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
            _logger.LogInformation("finished process: " + process.ExitCode);
            UpdateMessage updateFinished = new UpdateMessage
            {
                Id = taskMessage.TaskId,
                started_at = updateInProgress.started_at,
                finished_at = DateTime.UtcNow.ToUniversalTime(),
                status = Status.finished,
                exit_code = process.ExitCode,
                stderr = stderr,
                stdout = stdout
            };
            await _rabbitMessager.PublishUpdate(updateFinished);

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await _rabbitMessager.InitMessager();
            var taskQueueChannel = _rabbitMessager.GetTaskChannel();

            var consumer = new AsyncEventingBasicConsumer(taskQueueChannel!);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                var taskMessage = JsonConvert.DeserializeObject<TaskMessage>(messageJson);
                if (taskMessage != null)
                    await ProcessMessage(taskMessage);
            };

            await taskQueueChannel!.BasicConsumeAsync(queue: _rabbitMessager.GetTaskQueueName(), autoAck: true, consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

    }
}
