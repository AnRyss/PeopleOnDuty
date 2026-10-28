using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TaskStatus = PeopleOnDuty.src.Modules.Tasks.TaskStatus;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

public class CheckAuthorTasks(IUserService userService, ITaskService taskService, ITelegramBotClient bot) : IScenario
{
    public IEnumerable<string> HandledCallbacks => new[]
    {
        "author_task",          
        "complete_task",    
        "delete_task"        
    };

    public bool CanHandleInput(string? waitingForField) => false;

    public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
    {
        if (update.CallbackQuery is not { } callback) return;

        var chatId = callback.Message!.Chat.Id;
        var messageId = callback.Message.MessageId;
        var data = callback.Data;

        if (data is null) return;

        
        if (data == "author_task")
        {
            await RenderAuthorTasks(chatId, state, ct);
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

            try
            {
                await taskService.CompleteTaskAsync(taskId, ct);
                await bot.EditMessageText(chatId, messageId, "✅ Задача завершена!", cancellationToken: ct);
                await bot.AnswerCallbackQuery(callback.Id, "Задача завершена!", cancellationToken: ct);
            }
            catch (Exception ex)
            {
                await bot.AnswerCallbackQuery(callback.Id, ex.Message, showAlert: true, cancellationToken: ct);
            }
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

                if (task.Value.TaskStatus != TaskStatus.OPENED)
                {
                    await bot.AnswerCallbackQuery(callback.Id, "Нельзя удалить: задачу уже взяли.", showAlert: true, cancellationToken: ct);
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

    private async Task RenderAuthorTasks(long chatId, UserState state, CancellationToken ct)
    {
        var user = await userService.GetOrCreateByTelegramIdAsync(state.TelegramUserId, ct);
        var authorTasks = await taskService.GetByAuthorAsync(user.UserId, ct);

        if (authorTasks.Count == 0)
        {
            await bot.SendMessage(chatId, "У вас нет созданных задач.", cancellationToken: ct);
            return;
        }

        var buttons = authorTasks
            .Select(t => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{t.Name} — {t.Reward} ({t.TaskStatus})",
                    $"author_task:{t.TaskId}")
            })
            .ToArray();

        var keyboard = new InlineKeyboardMarkup(buttons);

        await bot.SendMessage(
            chatId,
            "Выберите задачу, которую хотите посмотреть:",
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    private async Task RenderTaskActions(long chatId, int messageId, Guid taskId, UserState state, CancellationToken ct)
    {
        var task = await taskService.GetTaskAsync(taskId, ct);
        if (task is null) return;

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