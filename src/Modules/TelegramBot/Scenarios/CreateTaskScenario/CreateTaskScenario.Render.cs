using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios
{
    public partial class CreateTaskScenario
    {
        
        public async Task RenderTaskCreationMenuAsync(long chatId, int messageId, UserState state, CancellationToken ct)
        {
            var text = $"""
        **Создание новой задачи**
        
        Имя: {state.DraftName ?? "не задано"}
        Описание: {state.DraftDescription ?? "не задано"}
        Награда: {state.DraftReward ?? "не задано"}
        Публичная: {(state.DraftIsPublic ? "Да" : "Нет")}
        """;

            var keyboard = new InlineKeyboardMarkup(new[]
            {
        new[] { InlineKeyboardButton.WithCallbackData("Поменять имя", "edit_name") },
        new[] { InlineKeyboardButton.WithCallbackData("Поменять описание", "edit_desc") },
        new[] { InlineKeyboardButton.WithCallbackData("Поменять награду", "edit_reward") },
        new[] { InlineKeyboardButton.WithCallbackData($"Публичность: {(state.DraftIsPublic ? "Да" : "Нет")}", "toggle_public") },
        new[] { InlineKeyboardButton.WithCallbackData("✅ Создать задачу", "confirm_create") },
        new[] { InlineKeyboardButton.WithCallbackData("❌ Отмена", "cancel_create") }
    });

            await _bot.EditMessageText(chatId, messageId, text, replyMarkup: keyboard, parseMode: ParseMode.Markdown, cancellationToken: ct);

        }
    }
}