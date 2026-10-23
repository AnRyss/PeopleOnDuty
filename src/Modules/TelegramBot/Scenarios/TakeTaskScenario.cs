using PeopleOnDuty.src.Modules.Shared;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios
{
    public class TakeTaskScenario(ITaskService _taskService, ITelegramBotClient _bot, IUserService _userService, IUserStateRepository _userStateRepository) : IScenario
    {
        public bool CanHandle(StateType state, Update update) => state is StateType.TakeTask;


        public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
        {    // 1. Выбор конкретной задачи
            if (update.CallbackQuery is not { } callback) return;
            if (callback.Data.StartsWith("take_task:"))
            {
                await HandleChosenTask(update, state, ct);
                state.CurrentState = StateType.MainMenu;
                await _userStateRepository.SaveAsync(state, ct);
                return; // ← ВЫХОД
            }



            var chatId = callback.Message!.Chat.Id;


            await RenderTakenTasks(chatId, state, ct);

            await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
        }
        public async Task HandleChosenTask(Update update, UserState state, CancellationToken ct)
        {
            if (update.CallbackQuery is not { } callback) return;
            if (callback.Data is null || !callback.Data.StartsWith("take_task:")) return;
            var user = await _userService.GetOrCreateByTelegramIdAsync(callback.From.Id, ct);
            var userGuid = user.UserId;
            Guid.TryParse(callback.Data.Substring("take_task:".Length), out var taskGuid);
            try
            {
                await _taskService.TakeTaskAsync(taskId: taskGuid, userId: userGuid, ct);
                await _bot.EditMessageText(
           chatId: callback.Message!.Chat.Id,
           messageId: callback.Message.Id,
           "✅ Вы взяли задачу! Свяжитесь с автором для уточнения деталей.",
           cancellationToken: ct);
                await _bot.AnswerCallbackQuery(callback.Id, "Задача взята!", cancellationToken: ct);
            }
            catch (InvalidOperationException ex)
            {

                await _bot.AnswerCallbackQuery(callback.Id, ex.Message, showAlert: true, cancellationToken: ct);
            }
            catch (Exception e)
            {
                await _bot.AnswerCallbackQuery(callback.Id, "Ошибка при взятии задачи.", showAlert: true, cancellationToken: ct);
            }
        }
        public async Task RenderTakenTasks(long chatId, UserState state, CancellationToken ct)
        {
            var openTasks = await _taskService.GetOpenTasksAsync(ct);

            if (openTasks.Count == 0)
            {
                state.CurrentState = StateType.MainMenu;
                await _userStateRepository.SaveAsync(state, ct);
                await _bot.SendMessage(chatId, "Сейчас нет открытых задач.", cancellationToken: ct);

                return;
            }

            var buttons = openTasks
                .Select(t => new[]
                {
            InlineKeyboardButton.WithCallbackData(
                $"{t.Name} — {t.Reward}",
                $"take_task:{t.TaskId}")
                })
                .ToArray();

            var keyboard = new InlineKeyboardMarkup(buttons);

            await _bot.SendMessage(
                chatId,
                "Выберите задачу, которую хотите взять:",
                replyMarkup: keyboard,
                cancellationToken: ct);
        }
    }
}



