using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;
using Telegram.Bot.Types;

namespace PeopleOnDuty.src.Modules.TelegramBot
{
    public interface IScenario
    {
        public bool CanHandle(StateType state, Update update);
        public  Task HandleAsync(Update update, UserState state, CancellationToken ct);

    }
}