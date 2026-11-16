using PeopleOnDuty.src.Modules.Shared;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

public partial class AdminScenario( 
    ITelegramBotClient bot,
    IConfiguration config,
    ITaskService taskService,
    IUserService userService,
    IUserStateRepository userStateRepository,
    IArchiveTaskService archiveTaskService,
    ILogger<AdminScenario> logger) : IScenario
{
  
    private readonly InlineKeyboardMarkup _adminKeyboard = new(new[]
    {
        new[] { InlineKeyboardButton.WithCallbackData("📊 Статистика", "admin_stats") },
        new[] { InlineKeyboardButton.WithCallbackData("🗑 Удалить незавершённые", "admin_unfinished_tasks")},
        new[] { InlineKeyboardButton.WithCallbackData("📢 Рассылка", "admin_broadcast") },
        new[] { InlineKeyboardButton.WithCallbackData("🗑 Удалить из архива", "admin_archive_tasks")}
    });

    public IEnumerable<string> HandledCallbacks => new[]
    {
        "admin_menu", "admin_stats", "admin_unfinished_tasks",
        "admin_delete_task", "admin_delete_confirm", "admin_broadcast", "admin_archive_tasks"
    };

    public bool CanHandleInput(string? waitingForField) => waitingForField is "admin_broadcast_text";

    public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
    {
        if (!await CheckAdminAccess(update, ct)) return;

        if (update.Message?.Text == "/admin")
        {
            await bot.SendMessage(update.Message.Chat.Id, "🛠 **Админ-панель**\nВыберите действие:",
                replyMarkup: _adminKeyboard, parseMode: ParseMode.Markdown, cancellationToken: ct);
            return;
        }

        // Обработка ввода текста для рассылки
        if (update.Message?.Text is { } text && state.WaitingForField == "admin_broadcast_text")
        {
            await ProcessBroadcastAsync(update, state, text, ct);
            return; 
        }

        // Обработка кнопок
        if (update.CallbackQuery is not { } callback) return;
        var chatId = callback.Message!.Chat.Id;
        var messageId = callback.Message.MessageId;

        switch (callback.Data)
        {
            case "admin_menu":
                await bot.EditMessageText(chatId, messageId, "🛠 **Админ-панель**\nВыберите действие:", replyMarkup: _adminKeyboard, parseMode: ParseMode.Markdown, cancellationToken: ct);
                break;
            case "admin_stats":
                await ShowStatsAsync(callback, ct);
                break;
            case "admin_unfinished_tasks":
                await ShowUnfinishedTasksAsync(callback, ct);
                break;
            case var data when data!.StartsWith("admin_delete_confirm:"):
                await ConfirmDeleteTaskAsync(callback, data, ct);
                break;
            case var data when data!.StartsWith("admin_delete_task:"):
                await DeleteTaskAsync(callback, data, state, ct);
                break;
            case "admin_broadcast":
                await StartBroadcastAsync(callback, state, ct);
                break;
            case "admin_archive_tasks":
                await ShowArchiveTasksAsync(callback, ct); 
                break;
        }

        await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
    }

    private async Task<bool> CheckAdminAccess(Update update, CancellationToken ct)
    {
        var userId = update.Message?.From?.Id ?? update.CallbackQuery?.From.Id;
        var adminIds = config.GetSection("Telegram:AdminIds").Get<string[]>() ?? Array.Empty<string>();

        if (userId == null || !adminIds.Contains(userId.ToString()))
        {
            if (update.Message != null)
                await bot.SendMessage(update.Message.Chat.Id, "⛔ У вас нет прав доступа.", cancellationToken: ct);
            return false;
        }
        return true;
    }
}