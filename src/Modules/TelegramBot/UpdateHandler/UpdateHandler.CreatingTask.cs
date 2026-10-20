using Telegram.Bot;
using Telegram.Bot.Types;

namespace PeopleOnDuty.src.Modules.TelegramBot
{
    public partial class UpdateHandler
    {
        private async Task HandleCreatingTaskNameAsync(Update update, UserState state, CancellationToken ct)
{
    // Нам нужен именно текст от пользователя
    if (update.Message?.Text is not { } text)
    {
        await _bot.SendMessage(update.Message!.Chat.Id, "Пожалуйста, введите название текстом.", cancellationToken: ct);
        return;
    }

    // Валидация (простая)
    if (string.IsNullOrWhiteSpace(text))
    {
        await _bot.SendMessage(update.Message.Chat.Id, "Название не должно быть пустым  Попробуйте еще раз.", cancellationToken: ct);
        return;
    }

    
    state.TempData = text;
    
    // Переводим в следующее состояние (ожидание награды)
    state.CurrentState = "CreatingTask_WaitingReward";
    await _userStateRepository.SaveAsync(state, ct);

    // Просим следующую порцию данных
    await _bot.SendMessage(
        update.Message.Chat.Id, 
        "Отлично! Теперь укажите вознаграждение (например, 'шоколадка' или 'ответная помощь'):", 
        cancellationToken: ct
    );
}
    }
}