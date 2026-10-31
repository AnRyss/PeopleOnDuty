using PeopleOnDuty.src.Modules.TelegramBot;
using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;
using Telegram.Bot.Types;

namespace PeopleOnDuty.src.Modules.TelegramBot.Handlers.UpdateHandler
{
    public class ScenarioRouter(IEnumerable<IScenario> _scenarios)
    {
        public async Task RouteAsync(Update update, UserState state, CancellationToken ct)
        {
            if (update.Message?.Text is { } text && text.StartsWith("/"))
            {
                var mainMenu = _scenarios.OfType<MainMenuScenario>().FirstOrDefault();
                if (mainMenu is not null)
                {
                    await mainMenu.HandleAsync(update, state, ct);
                    return; 
                }
            }
            var callbackHandled = false;
            // Сначала по коллбэкам
            if (update.CallbackQuery?.Data is { } data)
            {
                var scenarioByCallback = _scenarios.FirstOrDefault(s =>
                    s.HandledCallbacks.Contains(data) ||
                    s.HandledCallbacks.Any(c => data.StartsWith(c + ":")));

                if (scenarioByCallback is not null)
                {
                    callbackHandled = true;
                    await scenarioByCallback.HandleAsync(update, state, ct);

                }

            }
            //  И по инпутам
            if (callbackHandled is false)
            {
                var scenario = _scenarios.FirstOrDefault(s => s.CanHandleInput(state.WaitingForField));
                if (scenario is not null)
                    await scenario.HandleAsync(update, state, ct);
            }


        }
    }

}
