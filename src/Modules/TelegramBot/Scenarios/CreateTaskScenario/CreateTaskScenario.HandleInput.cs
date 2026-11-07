using Telegram.Bot;
using Telegram.Bot.Types;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios
{
    public partial class CreateTaskScenario
    {
        private async Task HandleTaskCreationInputAsync(Update update, UserState state, CancellationToken ct)
        {
            var chatId = update.Message!.Chat.Id;
            if (update.Message?.Text is not { } text) return;
            if (state.WaitingForField == "name" && text.Length > 40)
            {
                // Отправляем сообщение об ошибке
                await _bot.SendMessage( chatId,
                 "❌ Название задачи слишком длинное. Пожалуйста, введите название не более 40 символов.",
                    cancellationToken: ct);


                return;
            }

            // Сохраняем в нужное поле
            switch (state.WaitingForField)
            {
                case "name": state.DraftName = text; break;
                case "desc": state.DraftDescription = text; break;
                case "reward": state.DraftReward = text; break;
            }

            state.WaitingForField = null;
            await _userStateRepository.SaveAsync(state, ct);


            // В идеале нужно сохранять messageId меню в state, чтобы редактировать его, но для простоты  новое.

            var sentMsg = await _bot.SendMessage(chatId, "Обновление...", cancellationToken: ct);
            await RenderTaskCreationMenuAsync(chatId, sentMsg.Id, state, ct);
        }
    }
}