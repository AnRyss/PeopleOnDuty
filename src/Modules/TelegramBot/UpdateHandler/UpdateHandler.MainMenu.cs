using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot
{
    public partial class UpdateHandler
    {
        private async Task HandleMainMenuAsync(Update update, UserState state, CancellationToken ct)
{
    // Если это нажатие на кнопку (CallbackQuery)
    if (update.CallbackQuery is { } callback)
    {
        var chatId = callback.Message!.Chat.Id;
        var data = callback.Data;

        switch (data)
        {
            case "create_task":
                // Меняем состояние и просим название
                state.CurrentState = "CreatingTask_WaitingName";
                state.TempData = null; // Очищаем временные данные
                await _userStateRepository.SaveAsync(state, ct);

                await _bot.SendMessage(chatId, "Введите название задачи:", cancellationToken: ct);
                break;

            case "take_task":
                // TODO: Логика получения списка открытых задач
                await _bot.SendMessage(chatId, "Тут будет список задач (в разработке)", cancellationToken: ct);
                break;

            case "my_tasks":
                // TODO: Логика получения моих задач
                await _bot.SendMessage(chatId, "Тут будут ваши задачи (в разработке)", cancellationToken: ct);
                break;
        }

        
        await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
    }
    
    else if (update.Message?.Text == "/start")
    {
        var chatId = update.Message.Chat.Id;
        
        await _bot.SendMessage(
            chatId: chatId,
            text: "Привет! Что будем делать?",
            replyMarkup: new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Создать задачу", "create_task") },
                new[] { InlineKeyboardButton.WithCallbackData("Взять задачу", "take_task") },
                new[] { InlineKeyboardButton.WithCallbackData("Мои задачи", "my_tasks") }
            }),
            cancellationToken: ct
        );
    }
}
    }
}