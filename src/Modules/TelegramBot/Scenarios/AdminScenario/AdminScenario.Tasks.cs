using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TaskStatus = PeopleOnDuty.src.Modules.Tasks.TaskStatus;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

public partial class AdminScenario
{
    private async Task ShowUnfinishedTasksAsync(CallbackQuery callback, CancellationToken ct)
    {
        var allTasks = await taskService.GetAllTasksAsync(ct);
        var unfinished = allTasks.Where(t => t.TaskStatus == TaskStatus.OPENED || t.TaskStatus == TaskStatus.TAKEN).ToList();
        var chatId = callback.Message!.Chat.Id;
        var messageId = callback.Message.MessageId;

        if (!unfinished.Any())
        {
            var emptyKb = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", "admin_menu") } });
            await bot.EditMessageText(chatId, messageId, "Незавершённых задач нет.", replyMarkup: emptyKb, cancellationToken: ct);
            return;
        }

        var taskButtons = unfinished.Select(t => new[]
        {
            InlineKeyboardButton.WithCallbackData($"❌ {t.Name} [{t.TaskStatus}]", $"admin_delete_confirm:{t.TaskId}")
        }).ToList();

        taskButtons.Add(new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", "admin_menu") });

        await bot.EditMessageText(chatId, messageId, "🗑 **Нажмите на задачу, чтобы удалить её:**",
            replyMarkup: new InlineKeyboardMarkup(taskButtons), parseMode: ParseMode.Markdown, cancellationToken: ct);
    }

    private async Task ConfirmDeleteTaskAsync(CallbackQuery callback, string data, CancellationToken ct)
    {
        if (!Guid.TryParse(data.Substring("admin_delete_confirm:".Length), out var taskId)) return;

        var confirmKb = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("✅ Да, удалить", $"admin_delete_task:{taskId}") },
            new[] { InlineKeyboardButton.WithCallbackData("⬅️ Отмена", "admin_unfinished_tasks") }
        });
        await bot.EditMessageText(callback.Message!.Chat.Id, callback.Message.MessageId, 
            $"Вы уверены, что хотите удалить задачу `{taskId}`?", replyMarkup: confirmKb, parseMode: ParseMode.Markdown, cancellationToken: ct);
    }

    private async Task DeleteTaskAsync(CallbackQuery callback, string data, UserState state, CancellationToken ct)
    {
        if (!Guid.TryParse(data.Substring("admin_delete_task:".Length), out var idToDelete)) return;
        
        var chatId = callback.Message!.Chat.Id;
        var messageId = callback.Message.MessageId;
        try
        {
            await taskService.CancelTaskAsync(idToDelete, ct);
            await taskService.ChangeDescription(idToDelete, "ВМЕШАЛСЯ АДМИНИСТРАТОР", ct);
            var successKb = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("⬅️ К списку задач", "admin_unfinished_tasks") } });
            await bot.EditMessageText(chatId, messageId, "✅ Задача успешно отменена (удалена).", replyMarkup: successKb, cancellationToken: ct);
            logger.LogWarning("Админ {TgId} удалил(отменил) задачу {TaskId}!", state.TelegramUserId, idToDelete);
        }
        catch (Exception ex)
        {
            var errorKb = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", "admin_unfinished_tasks") } });
            await bot.EditMessageText(chatId, messageId, $"Ошибка: {ex.Message}", replyMarkup: errorKb, cancellationToken: ct);
        }
    }

    private async Task ShowArchiveTasksAsync(CallbackQuery callback, CancellationToken ct)
    {
        
        await bot.AnswerCallbackQuery(callback.Id, "В разработке", cancellationToken: ct);
    }
}