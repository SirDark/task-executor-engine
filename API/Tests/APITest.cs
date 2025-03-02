using Moq;
using API.Repositories;
using API.Controllers;
using API.Services.RabbitMQ;
using Microsoft.Extensions.Logging;
using API.Models.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Tests;

public class APITest
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IRabbitMQService> _rabbitMQServiceMock;
    private readonly Mock<ILogger<TasksController>> _loggerMock;
    private readonly TasksController _taskController;
    public APITest()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _rabbitMQServiceMock = new Mock<IRabbitMQService>();
        _loggerMock = new Mock<ILogger<TasksController>>();
       _taskController = new TasksController(_loggerMock.Object,_taskRepositoryMock.Object, _rabbitMQServiceMock.Object);
    }
    [Fact]
    public async Task GetTasks_ShouldReturnOkWithTasks_WhenThereAreTasksInTheDatabase()
    {
        //arrange
        EngineTask task = new EngineTask
        {
            id = Guid.NewGuid(),
            command = "command",
            stdout = "stdout",
            stderr = "stderr",
            status = "status",
            exitcode = 0,
            finished_at = DateTime.Now,
            started_at = DateTime.Now
        };
        List<EngineTask> tasks = new List<EngineTask>
        {
            task,
        };
        _taskRepositoryMock.Setup(s => s.GetAllAsync()).ReturnsAsync(tasks);

        //act
        var result = await _taskController.Get();
        //Assert
        result.Should().BeOfType<OkObjectResult>();
        var okresult = result as OkObjectResult;
        okresult!.Value.Should().BeEquivalentTo(tasks);
    }
    [Fact]
    public async Task GetTasks_ShouldReturnOkWithEmptyList_WhenThereAreNoTasksInTheDatabase()
    {
        //arrange
        List<EngineTask> tasks = new List<EngineTask>();
        _taskRepositoryMock.Setup(s => s.GetAllAsync()).ReturnsAsync(tasks);

        //act
        var result = await _taskController.Get();
        //Assert
        result.Should().BeOfType<OkObjectResult>();
        var okresult = result as OkObjectResult;
        okresult!.Value.Should().BeEquivalentTo(tasks);
    }
}