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
    IUserService userService) : IScenario
{
    // Обрабатываемые кнопки админки
    public IEnumerable<string> HandledCallbacks => new[]
    {
        "admin_menu",
        "admin_stats",
        "admin_unfinished_tasks",
        "admin_delete_task",
        "admin_delete_confirm"
    };

    public bool CanHandleInput(string? waitingForField) => false;

    public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
    {
        // 1. Проверка прав
        var userId = update.Message?.From?.Id ?? update.CallbackQuery?.From.Id;
        var adminIds = config.GetSection("Telegram:AdminIds").Get<string[]>() ?? Array.Empty<string>();
        Console.WriteLine($"adminsIds: {adminIds.ToString()}");
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
                new[] { InlineKeyboardButton.WithCallbackData("🗑 Удалить незавершённые", "admin_unfinished_tasks") }
            });
            await bot.SendMessage(update.Message.Chat.Id, "🛠 **Админ-панель**\nВыберите действие:", replyMarkup: keyboard, parseMode: ParseMode.Markdown, cancellationToken: ct);
            return;
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
                var tasks = await taskService.GetAllTasksAsync(ct);
                var usersCount = await userService.GetAllUsersCountAsync(ct);

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
                break;

            case "admin_unfinished_tasks":
                var allTasks = await taskService.GetAllTasksAsync(ct);
                // Берем только те, что не завершены и не отменены
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

            case var data when data.StartsWith("admin_delete_confirm:"):
                if (!Guid.TryParse(data.Substring("admin_delete_confirm:".Length), out var taskId)) break;

                var confirmKb = new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("✅ Да, удалить", $"admin_delete_task:{taskId}") },
                    new[] { InlineKeyboardButton.WithCallbackData("⬅️ Отмена", "admin_unfinished_tasks") }
                });
                await bot.EditMessageText(chatId, messageId, $"Вы уверены, что хотите удалить задачу `{taskId}`?", replyMarkup: confirmKb, parseMode: ParseMode.Markdown, cancellationToken: ct);
                break;

            case var data when data.StartsWith("admin_delete_task:"):
                if (!Guid.TryParse(data.Substring("admin_delete_task:".Length), out var idToDelete)) break;

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
                break;
        }

        await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
    }
}