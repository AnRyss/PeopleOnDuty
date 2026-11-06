using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TaskStatus = PeopleOnDuty.src.Modules.Tasks.TaskStatus;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

public class AdminScenario(
    ITelegramBotClient bot,
    IConfiguration config,
    ITaskService taskService,
    IUserService userService,
    IUserStateRepository userStateRepository) : IScenario
{

    public IEnumerable<string> HandledCallbacks => new[]
    {
        "admin_menu",
        "admin_stats",
        "admin_unfinished_tasks",
        "admin_delete_task",
        "admin_delete_confirm",
       "admin_broadcast"
    };

    public bool CanHandleInput(string? waitingForField) => waitingForField is "admin_broadcast_text";

    public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
    {
        // 1. Проверка прав
        var userId = update.Message?.From?.Id ?? update.CallbackQuery?.From.Id;
        var adminIds = config.GetSection("Telegram:AdminIds").Get<string[]>() ?? Array.Empty<string>();
        if (userId == null || !adminIds.Contains(userId.ToString()))
        {
            if (update.Message != null)
                await bot.SendMessage(update.Message.Chat.Id, "⛔ У вас нет прав доступа.", cancellationToken: ct);
            return;
        }


        if (update.Message?.Text == "/admin")
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("📊 Статистика", "admin_stats") },
                new[] { InlineKeyboardButton.WithCallbackData("🗑 Удалить незавершённые", "admin_unfinished_tasks")},
                new[] { InlineKeyboardButton.WithCallbackData("📢 Рассылка", "admin_broadcast") }
            });
            await bot.SendMessage(update.Message.Chat.Id, "🛠 **Админ-панель**\nВыберите действие:", replyMarkup: keyboard, parseMode: ParseMode.Markdown, cancellationToken: ct);
            return;
        }
        if (update.Message?.Text is { } text && state.WaitingForField == "admin_broadcast_text")
        {
            await BroadCast(update, state, text, ct);
        }
        // 3. Обработка кнопок
        if (update.CallbackQuery is not { } callback) return;
        var chatId = callback.Message!.Chat.Id;
        var messageId = callback.Message.MessageId;

        switch (callback.Data)
        {
            case "admin_menu":
                var menuKb = new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("📊 Статистика", "admin_stats") },
                    new[] { InlineKeyboardButton.WithCallbackData("🗑 Удалить незавершённые", "admin_unfinished_tasks") }
                });
                await bot.EditMessageText(chatId, messageId, "🛠 **Админ-панель**\nВыберите действие:", replyMarkup: menuKb, parseMode: ParseMode.Markdown, cancellationToken: ct);
                break;

            case "admin_stats":
                await ShowStats(callback, ct);
                break;

            case "admin_unfinished_tasks":
                var allTasks = await taskService.GetAllTasksAsync(ct);
                var unfinished = allTasks.Where(t => t.TaskStatus == Tasks.TaskStatus.OPENED || t.TaskStatus == TaskStatus.TAKEN).ToList();
                if (!unfinished.Any())
                {
                    var emptyKb = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", "admin_menu") } });
                    await bot.EditMessageText(chatId, messageId, "Незавершённых задач нет.", replyMarkup: emptyKb, cancellationToken: ct);
                    break;
                }

                var taskButtons = unfinished.Select(t => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"❌ {t.Name} [{t.TaskStatus}]",
                        $"admin_delete_confirm:{t.TaskId}")
                }).ToList();

                taskButtons.Add(new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", "admin_menu") });

                await bot.EditMessageText(chatId, messageId, "🗑 **Нажмите на задачу, чтобы удалить её:**",
                    replyMarkup: new InlineKeyboardMarkup(taskButtons), parseMode: ParseMode.Markdown, cancellationToken: ct);
                break;

            case var data when data.StartsWith("admin_delete_confirm:"): // подтверждение удаления
                if (!Guid.TryParse(data.Substring("admin_delete_confirm:".Length), out var taskId)) break;

                var confirmKb = new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("✅ Да, удалить", $"admin_delete_task:{taskId}") },
                    new[] { InlineKeyboardButton.WithCallbackData("⬅️ Отмена", "admin_unfinished_tasks") }
                });
                await bot.EditMessageText(chatId, messageId, $"Вы уверены, что хотите удалить задачу `{taskId}`?", replyMarkup: confirmKb, parseMode: ParseMode.Markdown, cancellationToken: ct);
                break;

            case var data when data!.StartsWith("admin_delete_task:"): // Удаление
                if (!Guid.TryParse(data.Substring("admin_delete_task:".Length), out var idToDelete)) break;
                await DeleteTask(callback, idToDelete, ct);

                break;
            case "admin_broadcast":
                state.WaitingForField = "admin_broadcast_text";
                await userStateRepository.SaveAsync(state, ct);
                await bot.EditMessageText(chatId, messageId, "Введите текст сообщения для рассылки:", cancellationToken: ct);
                break;
        }

        await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
    }
    private async Task BroadCast(Update update, UserState state, String text, CancellationToken ct)
    {
        int skip = 0;
        const int batchSize = 100;
        int success = 0;
        int failed = 0;
        state.WaitingForField = null;
        await userStateRepository.SaveAsync(state, ct);
        await bot.SendMessage(update.Message!.Chat.Id, "🚀 Рассылка запущена...", cancellationToken: ct);
        await Task.Run(async () =>
        {
            while (true)
            {
                var telegramIds = await userService.GetTelegramIdsBatchAsync(skip, batchSize, ct);

                if (telegramIds.Count == 0)
                    break;

                foreach (var tgId in telegramIds)
                {
                    try
                    {
                        await bot.SendMessage(tgId, text, cancellationToken: ct);
                        success++;
                        await Task.Delay(50, ct);
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ex) when (ex.ErrorCode == 429)
                    {
                        var retryAfter = ex.Parameters?.RetryAfter ?? 5;
                        await Task.Delay(TimeSpan.FromSeconds(retryAfter), CancellationToken.None);
                        failed++;
                    }
                    catch
                    {
                        failed++;
                    }
                }

                skip += batchSize;
            }
        });
        await bot.SendMessage(update.Message.Chat.Id, $"✅ Рассылка завершена.\nДоcтавлено: {success}\nОшибок: {failed}", cancellationToken: ct);
        return;
    }
    private async Task ShowStats(CallbackQuery callback, CancellationToken ct)
    {
        var tasks = await taskService.GetAllTasksAsync(ct);
        var usersCount = await userService.GetAllUsersCountAsync(ct);
        var chatId = callback.Message!.Chat.Id;
        var messageId = callback.Message.MessageId;

        var opened = tasks.Count(t => t.TaskStatus == TaskStatus.OPENED);
        var taken = tasks.Count(t => t.TaskStatus == TaskStatus.TAKEN);
        var completed = tasks.Count(t => t.TaskStatus == TaskStatus.COMPLETED);
        var cancelled = tasks.Count(t => t.TaskStatus == TaskStatus.CANCELLED);

        var statsText = $"""
                📊 **Статистика**
                
                👥 Пользователей: {usersCount}
                📝 Всего задач: {tasks.Count}
                
                🟢 Открыто: {opened}
                🟡 Взято: {taken}
                ✅ Завершено: {completed}
                ❌ Отменено: {cancelled}
                """;

        var backKb = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", "admin_menu") } });
        await bot.EditMessageText(chatId, messageId, statsText, replyMarkup: backKb, parseMode: ParseMode.Markdown, cancellationToken: ct);
    }
    private async Task DeleteTask(CallbackQuery callback, Guid idToDelete, CancellationToken ct)
    {
        var chatId = callback.Message!.Chat.Id;
        var messageId = callback.Message.MessageId;
        try
        {
            await taskService.CancelTaskAsync(idToDelete, ct);
            await taskService.ChangeDescription(idToDelete, "ВМЕШАЛСЯ АДМИНИСТРАТОР", ct);
            var successKb = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("⬅️ К списку задач", "admin_unfinished_tasks") } });
            await bot.EditMessageText(chatId, messageId, "✅ Задача успешно отменена (удалена).", replyMarkup: successKb, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            var errorKb = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", "admin_unfinished_tasks") } });
            await bot.EditMessageText(chatId, messageId, $"Ошибка: {ex.Message}", replyMarkup: errorKb, cancellationToken: ct);
        }
    }
}