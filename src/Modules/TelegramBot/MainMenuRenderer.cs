using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot
{
    public class MainMenuRenderer(ITelegramBotClient bot)
    {
       public readonly InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Создать задачу", "create_task") },
            new[] { InlineKeyboardButton.WithCallbackData("Взять задачу", "take_task") },
            new[] { InlineKeyboardButton.WithCallbackData("Взятые задачи", "taken_task") },
             new[] { InlineKeyboardButton.WithCallbackData("Мои созданные задачи", "author_task") },
        });

        public async Task SendAsync(long chatId, CancellationToken ct)
        {

            await bot.SendMessage(chatId, "Привет! Что будем делать?", replyMarkup: keyboard, cancellationToken: ct);
        }

        public async Task EditAsync(long chatId, int messageId, CancellationToken ct)
        {
            await bot.EditMessageText(chatId, messageId, "Привет! Что будем делать?", replyMarkup: keyboard, cancellationToken: ct);
        }
    }
}