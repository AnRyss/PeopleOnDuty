using System;
using PeopleOnDuty.src.Modules.TelegramBot.Handlers.UpdateHandler;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

public class MainMenuScenario(ITelegramBotClient _bot, IServiceProvider _services, IUserStateRepository userStateRepository) : IScenario
{
    public bool CanHandle(StateType state, Update update) =>
    state is StateType.MainMenu;


    public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
    {
        var scenarioRouter =
        _services.GetRequiredService<ScenarioRouter>();
        // 1. Обработка кнопок главного меню
        if (update.CallbackQuery is { } callback)
        {
            var chatId = callback.Message!.Chat.Id;

            switch (callback.Data)
            {
                case "create_task":
                    state.CurrentState = StateType.CreateTask;
                    state.DraftName = null;
                    state.DraftDescription = null;
                    state.DraftReward = null;
                    state.DraftIsPublic = false;
                    await userStateRepository.SaveAsync(state, ct);

                    // Добавляем задачу в роутер — он сам вызовет CreateTaskScenario
                    scenarioRouter.Enqueue(StateType.CreateTask, update, state);
                    break;
                // Взятие конкретной задачки тут вне рутинга, иначе вечный state = TakeTask. Подумаю потом
                case "take_task":
                    state.CurrentState = StateType.TakeTask;
                    await userStateRepository.SaveAsync(state, ct);
                    scenarioRouter.Enqueue(StateType.TakeTask, update, state);
                    break;

                case var data when data.StartsWith("take_task:"):
                    // Если пользователь нажал задачу из старого сообщения — состояние MainMenu
                    state.CurrentState = StateType.TakeTask;
                    await userStateRepository.SaveAsync(state, ct);
                    scenarioRouter.Enqueue(StateType.TakeTask, update, state);
                    break;

                case "taken_tasks":
                    state.CurrentState = StateType.CheckTakenTasks;
                    await userStateRepository.SaveAsync(state, ct);
                    scenarioRouter.Enqueue(StateType.CheckTakenTasks, update, state);
                    break;
            }

            await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
            return;
        }

        
    }
}