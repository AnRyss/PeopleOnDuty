using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TaskStatus = PeopleOnDuty.src.Modules.Tasks.TaskStatus;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

public partial class AdminScenario
{
    private async Task ShowStatsAsync(CallbackQuery callback, CancellationToken ct)
    {
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
        await bot.EditMessageText(callback.Message!.Chat.Id, callback.Message.MessageId, statsText, replyMarkup: backKb, parseMode: ParseMode.Markdown, cancellationToken: ct);
    }
}