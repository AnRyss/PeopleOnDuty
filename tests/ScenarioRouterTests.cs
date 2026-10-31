using FluentAssertions;
using Moq;
using PeopleOnDuty.src.Modules.TelegramBot;
using PeopleOnDuty.src.Modules.TelegramBot.Handlers.UpdateHandler;
using Telegram.Bot.Types;
using Xunit;

namespace PeopleOnDuty.Tests.TelegramBot;

public class ScenarioRouterTests
{
    [Fact]
    public async Task RouteAsync_WhenCallbackMatchesScenario_CallsIt()
    {
        var scenario = new Mock<IScenario>();
        scenario.Setup(s => s.HandledCallbacks).Returns(new[] { "create_task" });
        scenario.Setup(s => s.CanHandleInput(It.IsAny<string?>())).Returns(false);

        var router = new ScenarioRouter(new[] { scenario.Object });

        var update = new Update
        {
            CallbackQuery = new CallbackQuery
            {
                Data = "create_task",
                Message = new Message { Chat = new Chat { Id = 1 }, Id = 10 }
            }
        };

        await router.RouteAsync(update, new UserState { TelegramUserId = 1 }, default);

        scenario.Verify(s => s.HandleAsync(update, It.IsAny<UserState>(), default), Times.Once);
    }

    [Fact]
    public async Task RouteAsync_WhenTextAndCanHandleInput_CallsIt()
    {
        var scenario = new Mock<IScenario>();
        scenario.Setup(s => s.HandledCallbacks).Returns(Array.Empty<string>());
        scenario.Setup(s => s.CanHandleInput("archive_answer")).Returns(true);

        var router = new ScenarioRouter(new[] { scenario.Object });

        var update = new Update { Message = new Message { Text = "answer" } };

        await router.RouteAsync(update, new UserState { WaitingForField = "archive_answer" }, default);

        scenario.Verify(s => s.HandleAsync(update, It.IsAny<UserState>(), default), Times.Once);
    }
}