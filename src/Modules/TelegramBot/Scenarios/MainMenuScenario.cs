using System;
using PeopleOnDuty.src.Modules.TelegramBot.Handlers;
using PeopleOnDuty.src.Modules.TelegramBot.Handlers.UpdateHandler;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

public class MainMenuScenario(ITelegramBotClient _bot, PendingActionQueue _queue,
 IUserStateRepository userStateRepository) : IScenario
{
    public bool CanHandle(StateType state, Update update) =>
    state is StateType.MainMenu;


    public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
    {
      
        
        // 1. Обработка кнопок главного меню  TODO разделить на классы
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
                    _queue.Enqueue(StateType.CreateTask, update, state);
                    break;
                // Взятие конкретной задачки тут вне рутинга, иначе вечный state = TakeTask. Подумаю потом
                case var data when data.StartsWith("take_task"):
                    state.CurrentState = StateType.TakeTask;
                    await userStateRepository.SaveAsync(state, ct);
                    _queue.Enqueue(StateType.TakeTask, update, state);
                    break;
                case var data when data.StartsWith("taken_task"):
                    state.CurrentState = StateType.CheckTakenTasks;
                    await userStateRepository.SaveAsync(state, ct);
                    _queue.Enqueue(StateType.CheckTakenTasks, update, state);
                    break;
                    case "review":
                        await _bot.AnswerCallbackQuery(callback.Id,"Пока в разработке!",showAlert: true, cancellationToken: ct);
                    return;
                    
            }

            await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
            return;
        }

        
    }
}