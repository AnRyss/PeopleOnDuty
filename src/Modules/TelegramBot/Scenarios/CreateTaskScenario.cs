using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios
{
    public class CreateTaskScenario(ITelegramBotClient _bot, IUserStateRepository _userStateRepository, IUserService _userService, ITaskService _taskService) : IScenario
    {
        public bool CanHandle(StateType state, Update update) =>
         state is StateType.CreateTask or StateType.CreateTaskInput;


        public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
        {
            if (update.Message?.Text is { } text && state.CurrentState == StateType.CreateTaskInput)
            {
                await HandleTaskCreationInputAsync(update, state, ct);
                return;
            }

            if (update.CallbackQuery is not { } callback) return;

            var chatId = callback.Message!.Chat.Id;
            var messageId = callback.Message.MessageId;
            var data = callback.Data;

            switch (data)
            {
                case "create_task":
                    await RenderTaskCreationMenuAsync(chatId, messageId, state, ct);
                    break;
                case "edit_name":
                    state.CurrentState = StateType.CreateTaskInput;
                    state.WaitingForField = "name";
                    await _userStateRepository.SaveAsync(state, ct);
                    // Редактируем сообщение, убираем кнопки, просим текст
                    await _bot.EditMessageText(chatId, messageId, "Введите название задачи:", cancellationToken: ct);
                    break;

                case "edit_desc":
                    state.CurrentState = StateType.CreateTaskInput;
                    state.WaitingForField = "desc";
                    await _userStateRepository.SaveAsync(state, ct);
                    await _bot.EditMessageText(chatId, messageId, "Введите описание задачи:", cancellationToken: ct);
                    break;

                case "edit_reward":
                    state.CurrentState = StateType.CreateTaskInput;
                    state.WaitingForField = "reward";
                    await _userStateRepository.SaveAsync(state, ct);
                    await _bot.EditMessageText(chatId, messageId, "Введите награду:", cancellationToken: ct);
                    break;

                case "toggle_public":
                    state.DraftIsPublic = !state.DraftIsPublic;
                    await _userStateRepository.SaveAsync(state, ct);
                    // Просто перерисовываем меню с новым значением
                    await RenderTaskCreationMenuAsync(chatId, messageId, state, ct);
                    break;

                case "cancel_create":
                    state.CurrentState = StateType.MainMenu;
                    await _userStateRepository.SaveAsync(state, ct);
                    // Возвращаем главное меню (можно отредактировать это же сообщение)
                    await _bot.EditMessageText(chatId, messageId, "Создание отменено. Нажмите /start для выхода в меню.", cancellationToken: ct);
                    break;

                case "confirm_create":
                    if (string.IsNullOrWhiteSpace(state.DraftName) ||
                     string.IsNullOrWhiteSpace(state.DraftReward))
                    {
                        await _bot.AnswerCallbackQuery(callback.Id, "Заполните имя и награду!", showAlert: true, cancellationToken: ct);
                        return;
                    }
                    var userGuid = await _userService.GetOrCreateByTelegramIdAsync(callback.Message.Chat.Id, ct);
                    var createdTask = new TaskBuilder()
                    .WithName(state.DraftName)
                    .WithDescription(state.DraftDescription)
                    .WithReward(state.DraftReward)
                    .Build(userGuid.UserId);

                    await _taskService.CreateTaskAsync(createdTask, ct);

                    await _userStateRepository.SaveAsync(state, ct);
                    await _bot.EditMessageText(chatId, messageId, $"Задача '{state.DraftName}' создана!", cancellationToken: ct);
                    state.CurrentState = StateType.MainMenu;
                    state.DraftName = null;
                    state.DraftDescription = null;
                    state.DraftReward = null;
                    state.DraftIsPublic = false;

                    break;
            }

            await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
        }

        // Обработка текстового ввода (когда ждем имя, описание или награду)
        private async Task HandleTaskCreationInputAsync(Update update, UserState state, CancellationToken ct)
        {
            if (update.Message?.Text is not { } text) return;

            var chatId = update.Message.Chat.Id;


            // Сохраняем в нужное поле
            switch (state.WaitingForField)
            {
                case "name": state.DraftName = text; break;
                case "desc": state.DraftDescription = text; break;
                case "reward": state.DraftReward = text; break;
            }

            // Возвращаемся в меню
            state.CurrentState = StateType.CreateTask;
            state.WaitingForField = null;
            await _userStateRepository.SaveAsync(state, ct);

            // Отправляем НОВОЕ сообщение с меню (так как предыдущее мы отредактировали под запрос ввода, и вернуть его обратно сложно без messageId).
            // В идеале нужно сохранять messageId меню в state, чтобы редактировать его, но для простоты отправим новое.
            await _bot.SendMessage(chatId, "Данные сохранены!");

            // Чтобы отрисовать меню, нам нужен messageId. 
            // Для простоты отправим новое сообщение с меню. (В идеале - редактировать старое).
            var sentMsg = await _bot.SendMessage(chatId, "Обновление...", cancellationToken: ct);
            await RenderTaskCreationMenuAsync(chatId, sentMsg.MessageId, state, ct);
        }
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




