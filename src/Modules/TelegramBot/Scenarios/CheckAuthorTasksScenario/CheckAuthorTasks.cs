using PeopleOnDuty.src.Modules.Shared;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TaskStatus = PeopleOnDuty.src.Modules.Tasks.TaskStatus;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

public partial class CheckAuthorTasksScenario(IUserService userService, ITaskService taskService, IArchiveTaskService archiveTaskService,
ITelegramBotClient bot, IUserStateRepository userStateRepository,
MainMenuRenderer mainMenuRenderer, ILogger<CheckAuthorTasksScenario> logger) : IScenario
{
    public IEnumerable<string> HandledCallbacks => new[]
    {
        "author_task",
        "complete_task",
        "delete_task",
        "author_task_back"
    };

    public bool CanHandleInput(string? waitingForField) => waitingForField is "archive_answer";

    public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
    {
        if (update.Message?.Text is { } text)
        {
            if (state.WaitingForField == "archive_answer")
            {
                await HandleArchiveAnswerAsync(update, state, text, ct);
                return;
            }
        }
        if (update.CallbackQuery is not { } callback) return;

        var chatId = callback.Message!.Chat.Id;
        var messageId = callback.Message.MessageId;
        var data = callback.Data;

        if (data is null) return;


        if (data == "author_task")
        {
            await RenderAuthorTasks(chatId, messageId, state, ct);
            await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
            return;
        }
        if (data == "author_task_back")
        {
            await mainMenuRenderer.EditAsync(chatId, messageId, ct);
            await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
            return;
        }


        if (data.StartsWith("author_task:"))
        {
            if (!Guid.TryParse(data.Substring("author_task:".Length), out var taskId)) return;
            await RenderTaskActions(chatId, messageId, taskId, state, ct);
            await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
            return;
        }


        if (data.StartsWith("complete_task:"))
        {
            if (!Guid.TryParse(data.Substring("complete_task:".Length), out var taskId)) return;

            await HandleCompleteTaskAsync(chatId, messageId, taskId, state, ct);
            return;
        }


        if (data.StartsWith("delete_task:"))
        {
            if (!Guid.TryParse(data.Substring("delete_task:".Length), out var taskId)) return;

            try
            {
                var task = await taskService.GetTaskAsync(taskId, ct);
                if (task is null)
                {
                    await bot.AnswerCallbackQuery(callback.Id, "Задача не найдена.", showAlert: true, cancellationToken: ct);
                    return;
                }



                await taskService.CancelTaskAsync(taskId, ct);
                await bot.EditMessageText(chatId, messageId, "🗑 Задача отменена.", cancellationToken: ct);
                await bot.AnswerCallbackQuery(callback.Id, "Задача отменена!", cancellationToken: ct);
            }
            catch (Exception ex)
            {
                await bot.AnswerCallbackQuery(callback.Id, ex.Message, showAlert: true, cancellationToken: ct);
            }
            return;
        }
    }

    private async Task RenderAuthorTasks(long chatId, int messageId, UserState state, CancellationToken ct)
    {
        var mainMenuButton = new InlineKeyboardButton("⬅️ В главное меню", callbackDataOrUrl: "author_task_back");
        var mainMenuKeyboard = new InlineKeyboardMarkup(mainMenuButton);
        var user = await userService.GetOrCreateByTelegramIdAsync(state.TelegramUserId, ct);
        var authorTasks = await taskService.GetByAuthorAsync(user.UserId, ct);

        if (authorTasks.Count == 0)
        {
            await bot.EditMessageText(chatId, messageId, "У вас нет созданных задач.", replyMarkup: mainMenuKeyboard, cancellationToken: ct);
            return;
        }

        var buttons = authorTasks
        .Where(t => t.TaskStatus != TaskStatus.CANCELLED)
            .Select(t => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{t.Name} ({t.TaskStatus})",
                    $"author_task:{t.TaskId}")
            })
            .ToList();
        buttons.Add(new[] { mainMenuButton });
        var keyboard = new InlineKeyboardMarkup(buttons);

        await bot.EditMessageText(
            chatId, messageId,
            "Выберите задачу, которую хотите посмотреть:",
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    private async Task RenderTaskActions(long chatId, int messageId, Guid taskId, UserState state, CancellationToken ct)
    {
        var user = await userService.GetOrCreateByTelegramIdAsync(state.TelegramUserId, ct);
        var task = await taskService.GetTaskAsync(taskId, ct);
        if (task is null || task.Value.AuthorId != user.UserId)
        {
            await bot.AnswerCallbackQuery("Задача не найдена или у вас нет прав.", showAlert: true, cancellationToken: ct);
            return;
        }
        var t = task.Value;

        var text = $"""
        **Задача: {t.Name}**
        
        **Описание:** {t.Description ?? "Не задано"}
        **Награда:** {t.Reward}
        **Статус:** {t.TaskStatus}
        """;

        var buttons = new List<InlineKeyboardButton[]>();


        if (t.TaskStatus == TaskStatus.TAKEN)
        {
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("✅ Завершить", $"complete_task:{t.TaskId}")
            });
        }
        if (t.TaskStatus == TaskStatus.OPENED)
        {
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("🗑 Удалить", $"delete_task:{t.TaskId}")
            });
        }
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("⬅️ Назад к списку", "author_task")
        });

        var keyboard = new InlineKeyboardMarkup(buttons);

        await bot.EditMessageText(
            chatId,
            messageId,
            text,
            replyMarkup: keyboard,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

}