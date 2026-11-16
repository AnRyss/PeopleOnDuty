using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.TelegramBot;
using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;
using PeopleOnDuty.src.Modules.Users;
using Telegram.Bot;
using Telegram.Bot.Types;
using Xunit;

namespace PeopleOnDuty.Tests.Scenarios;

public class CheckAuthorTasksScenarioTests
{
    [Fact]
    public async Task HandleAsync_WhenCompletePublicTask_AsksForAnswer()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var telegramId = 1L;

        var taskDto = new TaskDto(
            Name: "Test Task",
            Description: "Desc",
            Reward: "100",
            AuthorId: userId, 
            TaskId: taskId,
            TaskStatus: src.Modules.Tasks.TaskStatus.TAKEN,
            IsPublic: true,
            CreatedAt: DateTime.UtcNow
        );

        var botMock = new Mock<ITelegramBotClient>();
        var tasksMock = new Mock<ITaskService>();
        var usersMock = new Mock<IUserService>();
        var statesMock = new Mock<IUserStateRepository>();
        var archiveTasksMock = new Mock<IArchiveTaskService>();
        var loggerMock = new Mock<ILogger<CheckAuthorTasksScenario>>();

        
        var mainMenuRendererMock = new Mock<MainMenuRenderer>(MockBehavior.Loose, botMock.Object);

        var sut = new CheckAuthorTasksScenario(
            usersMock.Object,
            tasksMock.Object,
            archiveTasksMock.Object,
            botMock.Object,
            statesMock.Object,
            mainMenuRendererMock.Object,
            loggerMock.Object);

        // Настраиваем моки, чтобы не было NullReferenceException
        usersMock.Setup(x => x.GetOrCreateByTelegramIdAsync(telegramId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new src.Modules.Users.User("TestUser") { UserId = userId }); // Возвращаем юзера с нужным ID

        tasksMock.Setup(x => x.GetTaskAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskDto);

        // Формируем Update
        var update = new Update
        {
            CallbackQuery = new CallbackQuery
            {
                Id = "cb-id", 
                Data = $"complete_task:{taskId}",
                Message = new Message { Chat = new Chat { Id = 1 }, Id = 10 }
            }
        };

        var state = new UserState { TelegramUserId = telegramId };

        // Act
        await sut.HandleAsync(update, state, default);

        // Assert
        state.WaitingForField.Should().Be("archive_answer");
        state.PendingTaskId.Should().Be(taskId.ToString());
        statesMock.Verify(s => s.SaveAsync(state, It.IsAny<CancellationToken>()), Times.Once);
    }
}