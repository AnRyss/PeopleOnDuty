using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios
{
    public class CheckTakenTasks(ITaskService taskService, IUserService userService, ITelegramBotClient bot) : IScenario
    {
        public IEnumerable<string> HandledCallbacks => new[] { "taken_task" };
        // public bool CanHandle(StateType state, Update update) =>
        //state is StateType.CheckTakenTasks;
        public bool CanHandleInput(string? waitingForField) => false;


        public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
        {
            if (update.CallbackQuery is not { } callback) return;
            if (callback.Data!.StartsWith("taken_task:"))
            {
                await HandleChosenTakenTask(update, state, ct);
                return;
            }
            var chatId = callback.Message!.Chat.Id;
            var messageId = callback.Message.MessageId;
            var data = callback.Data;
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
        InlineKeyboardButton.WithCallbackData("⬅️ Назад к списку", "taken_task")
    });

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

            var takenTasks = await taskService.GetTakenTasksByUserAsync(user.UserId, ct);

            if (takenTasks.Count == 0)
            {
                await bot.SendMessage(chatId, "У вас пока нет взятых задач.", cancellationToken: ct);
                return;
            }

            var buttons = takenTasks
                .Select(t => new[]
                {
            InlineKeyboardButton.WithCallbackData(
                $"{t.Name} — {t.Reward}",
                $"taken_task:{t.TaskId}")
                })
                .ToArray();

            var keyboard = new InlineKeyboardMarkup(buttons);

            await bot.SendMessage(
                chatId,
                "Выберите задачу, которую хотите посмотреть:",
                replyMarkup: keyboard,
                cancellationToken: ct);
        }
    }
}
