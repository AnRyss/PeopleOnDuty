using PeopleOnDuty.src.Modules.TelegramBot;
using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;
using Telegram.Bot.Types;

namespace PeopleOnDuty.src.Modules.TelegramBot.Handlers.UpdateHandler
{
    public class ScenarioRouter(IEnumerable<IScenario> scenarios, IUserStateRepository userStateRepository)
    {
        public async Task RouteAsync(Update update, UserState state, CancellationToken ct)
        {

            if (update.Message?.Text is { } text && text.StartsWith("/"))
            {
                if (update.Message?.Text == "/admin")
                {
                    var adminScenario = scenarios.OfType<AdminScenario>().FirstOrDefault();
                    if (adminScenario is not null)
                    {
                        await adminScenario.HandleAsync(update, state, ct);
                        
                    }
                    return;
                }
                    var mainMenu = scenarios.OfType<MainMenuScenario>().FirstOrDefault();
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
                    state.ResetAllButDraft();
                    await userStateRepository.SaveAsync(state,ct);
                    var scenarioByCallback = scenarios.FirstOrDefault(s =>
                        s.HandledCallbacks.Contains(data) ||
                        s.HandledCallbacks.Any(c => data.StartsWith(c + ":")));

                    if (scenarioByCallback is not null)
                    {
                        callbackHandled = true;
                        await scenarioByCallback.HandleAsync(update, state, ct);
                        return;
                    }

                }
                //  И по инпутам
                if (callbackHandled is false)
                {
                    var scenario = scenarios.FirstOrDefault(s => s.CanHandleInput(state.WaitingForField));
                    if (scenario is not null)
                        await scenario.HandleAsync(update, state, ct);
                }


            }
        }

    }
