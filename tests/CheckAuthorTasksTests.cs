using FluentAssertions;
using Moq;
using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Tasks;
using PeopleOnDuty.src.Modules.TelegramBot;
using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Types;
using Xunit;

namespace PeopleOnDuty.Tests.Scenarios;

public class CheckAuthorTasksScenarioTests
{
    [Fact]
    public async Task HandleAsync_WhenCompletePublicTask_AsksForAnswer()
    {
        var taskId = Guid.NewGuid();
        var tasks = new Mock<ITaskService>();
        tasks.Setup(t => t.GetTaskAsync(taskId, default))
             .ReturnsAsync(new TaskDto(
                 "name", "desc", "reward",
                 Guid.NewGuid(), taskId,
                 PeopleOnDuty.src.Modules.Tasks.TaskStatus.TAKEN,
                 isPublic: true));

        var bot = new Mock<ITelegramBotClient>();
        var users = new Mock<IUserService>();
        var states = new Mock<IUserStateRepository>();

        var sut = new CheckAuthorTasksScenario(
            users.Object, tasks.Object, bot.Object, states.Object);

        var update = new Update
        {
            CallbackQuery = new CallbackQuery
            {
                Id = "cb",
                Data = $"complete_task:{taskId}",
                Message = new Message { Chat = new Chat { Id = 1 }, Id = 10 }
            }
        };

        var state = new UserState { TelegramUserId = 1 };

        await sut.HandleAsync(update, state, default);

        state.WaitingForField.Should().Be("archive_answer");
        state.PendingTaskId.Should().Be(taskId.ToString());
        states.Verify(s => s.SaveAsync(state, default), Times.Once);
    }
}