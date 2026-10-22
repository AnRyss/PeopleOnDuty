using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot
{
    public partial class UpdateHandler
    {
        private async Task HandleMainMenuAsync(Update update, UserState state, CancellationToken ct)
{
   if (update.CallbackQuery is { } callback)
    {
        var chatId = callback.Message!.Chat.Id;
        var messageId = callback.Message.MessageId;
        var data = callback.Data;

        switch (data)
        {
            case "create_task":
                // Инициализируем черновик
                state.CurrentState = "CreatingTask_Menu";
                state.DraftName = null;
                state.DraftDescription = null;
                state.DraftReward = null;
                state.DraftIsPublic = false;
                await _userStateRepository.SaveAsync(state, ct);

                await RenderTaskCreationMenuAsync(chatId, messageId, state, ct);
                break;

            case "take_task":
                await _bot.SendMessage(chatId, "Тут будет список задач (в разработке)", cancellationToken: ct);
                break;
                
            case "my_tasks":
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
    else if (update.Message?.Text == "/tell_about")
            {
                await _bot.SendMessage(chatId : update.Message.Chat.Id,
                                       text: "Бот гарантирует необходимый уровень безопасности, например, при создании просьбы вы можете указать публичность(Будет ли сохранена в публичный архим(анонимно) ваша просьба)"
                                       );
            }
}
    }
}