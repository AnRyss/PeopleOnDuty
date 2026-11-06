using System.Collections.Immutable;
using PeopleOnDuty.src.Modules.Shared;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios
{
    public class TakeTaskScenario(ITaskService taskService, ITelegramBotClient bot,
    IUserService userService, IUserStateRepository userStateRepository, MainMenuRenderer mainMenuRenderer) : IScenario
    {
        public IEnumerable<string> HandledCallbacks => new[] { "take_task", "take_task_back" };

        public bool CanHandleInput(string? waitingForField) => false;



        public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
        {    //  Выбор конкретной задачи
            if (update.CallbackQuery is not { } callback || callback.Data is null) return;
            var chatId = callback.Message!.Chat.Id;
            var messageId = callback.Message.Id;
            if (callback.Data == "take_task_back")
            {
                await mainMenuRenderer.EditAsync(chatId, messageId, ct);
                await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                return;
            }
            if (callback.Data.StartsWith("take_task:"))
            {
                await HandleChosenTask(update, state, ct);

                await userStateRepository.SaveAsync(state, ct);
                return;
            }
            // Отображение списка задач

            await RenderTakenTasks(chatId, state, ct);
            await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
        }
        public async Task HandleChosenTask(Update update, UserState state, CancellationToken ct)
        {
            if (update.CallbackQuery is not { } callback) return;
            if (callback.Data is null || !callback.Data.StartsWith("take_task:")) return;
            var user = await userService.GetOrCreateByTelegramIdAsync(callback.From.Id, ct);
            var userGuid = user.UserId;
            Guid.TryParse(callback.Data.Substring("take_task:".Length), out var taskGuid);
            try
            {
                await taskService.TakeTaskAsync(taskId: taskGuid, userId: userGuid, ct);
                await bot.EditMessageText(
           chatId: callback.Message!.Chat.Id,
           messageId: callback.Message.Id,
           "✅ Вы взяли задачу! Свяжитесь с автором через меню взятых задач.",
           cancellationToken: ct);
                await bot.AnswerCallbackQuery(callback.Id, "Задача взята!", cancellationToken: ct);
            }
            catch (InvalidOperationException ex)
            {

                await bot.AnswerCallbackQuery(callback.Id, ex.Message, showAlert: true, cancellationToken: ct);
            }

        }
        public async Task RenderTakenTasks(long chatId, UserState state, CancellationToken ct)
        {
            var mainMenuButton = new InlineKeyboardButton("⬅️ В главное меню", callbackDataOrUrl: "take_task_back");
            var user = await userService.GetOrCreateByTelegramIdAsync(state.TelegramUserId, ct);
            var openTasks = await taskService.GetByStatusAsync(Tasks.TaskStatus.OPENED, ct);


            if (openTasks.Count == 0)
            {
                //state.CurrentState = StateType.MainMenu;

                await userStateRepository.SaveAsync(state, ct);
                await bot.SendMessage(chatId, "Сейчас нет открытых задач.", replyMarkup: mainMenuButton, cancellationToken: ct);

                return;
            }

            var buttons = openTasks
                .Select(t => new[]
                {
            InlineKeyboardButton.WithCallbackData(
                $"{t.Name}",
                $"take_task:{t.TaskId}")
                })
                .ToArray();

            var keyboard = new InlineKeyboardMarkup(buttons);
            keyboard.AddButton(mainMenuButton);
            await bot.SendMessage(
                chatId,
                "Выберите задачу, которую хотите взять:",
                replyMarkup: keyboard,
                cancellationToken: ct);
        }
    }
}



