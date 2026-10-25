using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;
using Telegram.Bot.Types;

namespace PeopleOnDuty.src.Modules.TelegramBot
{
    public interface IScenario
    {
        public IEnumerable<string> HandledCallbacks { get; }
        public bool CanHandleInput(string? waitingForField);
        public Task HandleAsync(Update update, UserState state, CancellationToken ct);

    }
}