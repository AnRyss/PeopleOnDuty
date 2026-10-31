using PeopleOnDuty.src.Modules.Shared;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

public class ArchiveTaskScenario(
    IUserStateRepository userStateRepository,
    ITaskService taskService,
    IArchiveTaskService archiveTaskService,
    ITelegramBotClient bot) : IScenario
{
    private const int PageSize = 5;

    public IEnumerable<string> HandledCallbacks => new[]
    {
        "archive",          // открыть архив
        "archive_page",     // archive_page:{page} — навигация
        "archive_search",   // archive_search — запросить поиск
        "archive_back"      // вернуться к списку (без поиска)
    };

    public bool CanHandleInput(string? waitingForField) =>
        waitingForField is "archive_answer" or "archive_query";

    public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
    {
        // 1. Текст: ответ на задачу при завершении задачи или поисковый запрос
        if (update.Message?.Text is { } text)
        {
            if (state.WaitingForField == "archive_answer")
            {
                await HandleArchiveAnswerAsync(update, state, text, ct);
                return;
            }

            if (state.WaitingForField == "archive_query")
            {
                state.WaitingForField = null;
                await userStateRepository.SaveAsync(state, ct);
                await RenderArchiveAsync(update.Message.Chat.Id, messageId: null, query: text, page: 1, ct);
                return;
            }
        }

        // 2. Кнопки
        if (update.CallbackQuery is not { } callback) return;
        var chatId = callback.Message!.Chat.Id;
        var messageId = callback.Message.MessageId;
        var data = callback.Data;

        if (data == "archive")
        {
            await RenderArchiveAsync(chatId, messageId, query: null, page: 1, ct);
            await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
            return;
        }

        if (data.StartsWith("archive_page:"))
        {
            if (!int.TryParse(data.Substring("archive_page:".Length), out var page)) return;
            await RenderArchiveAsync(chatId, messageId, query: null, page, ct);
            await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
            return;
        }

        if (data == "archive_search")
        {
            state.WaitingForField = "archive_query";
            await userStateRepository.SaveAsync(state, ct);
            await bot.EditMessageText(chatId, messageId, "Введите название задачи для поиска:", cancellationToken: ct);
            await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
            return;
        }

        if (data == "archive_back")
        {
            await RenderArchiveAsync(chatId, messageId, query: null, page: 1, ct);
            await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
            return;
        }
    }

    private async Task RenderArchiveAsync(
        long chatId, int? messageId, string? query, int page, CancellationToken ct)
    {
        var (items, total) = await archiveTaskService.SearchAsync(query, page, PageSize, ct);
        var totalPages = (int)Math.Ceiling(total / (double)PageSize);
        if (totalPages == 0) totalPages = 1;

        var header = string.IsNullOrWhiteSpace(query)
            ? "📚 **Архив задач**"
            : $"🔍 **Поиск:** «{query}»";

        var text = $"""
        {header}
        
        Найдено: {total} (стр. {page}/{totalPages})
        """;

        if (items.Count == 0)
            text += "\n\nНет задач.";

        foreach (var item in items)
        {
            text += $"\n\n**{item.TaskName}** — {item.Reward}\n_{item.Answer}_";
        }

        var buttons = new List<InlineKeyboardButton[]>();

        // Навигация
        if (totalPages > 1)
        {
            var nav = new List<InlineKeyboardButton>();
            if (page > 1)
                nav.Add(InlineKeyboardButton.WithCallbackData("⬅️", $"archive_page:{page - 1}"));
            if (page < totalPages)
                nav.Add(InlineKeyboardButton.WithCallbackData("➡️", $"archive_page:{page + 1}"));
            if (nav.Count > 0) buttons.Add(nav.ToArray());
        }

        // Поиск / сброс
        if (string.IsNullOrWhiteSpace(query))
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("🔍 Поиск по названию", "archive_search") });
        else
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("⬅️ Ко всему архиву", "archive_back") });

        var keyboard = new InlineKeyboardMarkup(buttons);

        if (messageId.HasValue)
            await bot.EditMessageText(chatId, messageId.Value, text, replyMarkup: keyboard, parseMode: ParseMode.Markdown, cancellationToken: ct);
        else
            await bot.SendMessage(chatId, text, replyMarkup: keyboard, parseMode: ParseMode.Markdown, cancellationToken: ct);
    }

    private async Task HandleArchiveAnswerAsync(
        Update update, UserState state, string answer, CancellationToken ct)
    {
        if (state.PendingTaskId is not { } taskIdString) return;
        if (!Guid.TryParse(taskIdString, out var taskId)) return;

        var task = await taskService.GetTaskAsync(taskId, ct);
        if (task is null) return;

        var dto = new ArchivedTaskDto
        {
            OriginalTaskId = task.Value.TaskId,
            TaskName = task.Value.Name,
            TaskDescription = task.Value.Description,
            Reward = task.Value.Reward,
            AuthorId = task.Value.AuthorId,
            Answer = answer,
            CompletedAt = DateTime.UtcNow
        };

        await archiveTaskService.SaveAsync(dto, ct);
        await taskService.CompleteTaskAsync(taskId, ct);

        state.WaitingForField = null;
        state.PendingTaskId = null;
        await userStateRepository.SaveAsync(state, ct);

        await bot.SendMessage(update.Message!.Chat.Id, "✅ Задача завершена и сохранена в архив!", cancellationToken: ct);
    }
}