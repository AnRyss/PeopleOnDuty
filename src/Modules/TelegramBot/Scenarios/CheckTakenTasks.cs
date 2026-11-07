using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios
{
    public class CheckTakenTasks(ITaskService taskService, IUserService userService, ITelegramBotClient bot,
    MainMenuRenderer mainMenuRenderer) : IScenario
    {
        public IEnumerable<string> HandledCallbacks => new[] { "taken_task", "taken_task_back", "taken_task_abandon" };
        // public bool CanHandle(StateType state, Update update) =>
        //state is StateType.CheckTakenTasks;
        public bool CanHandleInput(string? waitingForField) => false;


        public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
        {
            if (update.CallbackQuery is not { } callback) return;
            var chatId = callback.Message!.Chat.Id;
            var messageId = callback.Message.MessageId;
            if (callback.Data == "taken_task_back")
            {
                await mainMenuRenderer.EditAsync(chatId, messageId, ct);
                await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                return;
            }
            if (callback.Data!.StartsWith("taken_task:"))
            {
                await HandleChosenTakenTask(update, state, ct);
                return;
            }
            if (callback.Data.StartsWith("taken_task_abandon:"))
            {
                Guid.TryParse(callback.Data.Substring("taken_task_abandon:".Length), out var taskId);
                var user = await userService.GetOrCreateByTelegramIdAsync(state.TelegramUserId, ct);
                await taskService.AbandonTaskAsync(taskId, user.UserId, ct);
                await bot.DeleteMessage(chatId, messageId, ct);
                await bot.AnswerCallbackQuery(callback.Id, text: "Вы отказались от задачи", showAlert: true, cancellationToken: ct);
                return;
            }
            await RenderTakenTasks(chatId, state, ct);
            //state.CurrentState = StateType.MainMenu;
            //await userStateRepository.SaveAsync(state, ct);
            await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);

        }
        public async Task HandleChosenTakenTask(Update update, UserState state, CancellationToken ct)
        {
            if (update.CallbackQuery is not { } callback || callback.Data is null) return;
            if (!callback.Data.StartsWith("taken_task:")) return;
            Guid.TryParse(callback.Data.Substring("taken_task:".Length), out var taskId);
            var task = await taskService.GetTaskAsync(taskId, ct);

            await RenderChosenTakenTask(task, callback.Message!.Chat.Id, callback.Message.Id, ct);
        }


        private async Task RenderChosenTakenTask(TaskDto? task, long chatId, int messageId, CancellationToken ct)
        {
            if (task is null) return;


            var text = $"""
    **Задача: {task.Value.Name ?? "Ошибка. Не задано"}**
    
    **Описание:** {task.Value.Description ?? "Не задано"}
    **Награда:** {task.Value.Reward ?? "Ошибка. Не задано"}
    **Статус:** {task.Value.TaskStatus.ToString()}
    """;

            var buttons = new List<InlineKeyboardButton[]>();
            var author = await userService.GetUserAsync(task.Value.AuthorId, ct);
            var tgAuthorId = author?.TelegramId;

            if (tgAuthorId is null)
            {
                buttons.Add(new[]
                   {
            InlineKeyboardButton.WithDisabled(
                "👤 (Ошибка, нет ссылки на автора")
        });
            }
            else
            {
                buttons.Add(new[]
                {
            InlineKeyboardButton.WithUrl(
                "👤 ()Профиль автора",
                $"tg://user?id={tgAuthorId}")
        });
            }


            buttons.Add(new[]
            {
        InlineKeyboardButton.WithCallbackData("Отказаться от задачи", $"taken_task_abandon:{task.Value.TaskId}"),
        InlineKeyboardButton.WithCallbackData("⬅️ Назад к списку", "taken_task")});

            var keyboard = new InlineKeyboardMarkup(buttons);

            await bot.EditMessageText(
                chatId,
                messageId,
                text,
                replyMarkup: keyboard,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                cancellationToken: ct);
        }


        public async Task RenderTakenTasks(long chatId, UserState state, CancellationToken ct)
        {
            var user = await userService.GetOrCreateByTelegramIdAsync(state.TelegramUserId, ct);
            var toMainMenuButton = new InlineKeyboardButton("⬅️ В главное меню", "taken_task_back");
            var takenTasks = await taskService.GetTakenTasksByUserAsync(user.UserId, ct);

            if (takenTasks.Count == 0)
            {
                await bot.SendMessage(chatId, "У вас пока нет взятых задач.", replyMarkup: toMainMenuButton, cancellationToken: ct);
                return;
            }

            var buttons = takenTasks
                .Where(t => t.TaskStatus != Tasks.TaskStatus.CANCELLED && t.TaskStatus != Tasks.TaskStatus.COMPLETED)
                .Select(t => new[]
                {
            InlineKeyboardButton.WithCallbackData(
                $"{t.Name}",
                $"taken_task:{t.TaskId}")
                })
                .ToList();
            var mainMenuButton = new InlineKeyboardButton("⬅️ В главное меню", "taken_task_back");
            buttons.Add(new[] { mainMenuButton });
            var keyboard = new InlineKeyboardMarkup(buttons);

            await bot.SendMessage(
                chatId,
                "Выберите задачу, которую хотите посмотреть:",
                replyMarkup: keyboard,
                cancellationToken: ct);
        }
    }
}
