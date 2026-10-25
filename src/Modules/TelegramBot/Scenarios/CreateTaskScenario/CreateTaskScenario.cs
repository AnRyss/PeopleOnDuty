using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Tasks;
using PeopleOnDuty.src.Modules.TelegramBot.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios
{
    public partial class CreateTaskScenario(ITelegramBotClient _bot, IUserStateRepository _userStateRepository,
     IUserService _userService, ITaskService _taskService, MainMenuRenderer mainMenuRenderer) : IScenario
    {
        public bool CanHandleInput(string? waitingForField) =>
    waitingForField is "name" or "desc" or "reward";
        public IEnumerable<string> HandledCallbacks => new[]
  {
    "create_task", "edit_name", "edit_desc", "edit_reward",
    "toggle_public", "confirm_create", "cancel_create"
};


        public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
        {
            if (update.Message?.Text is { } text && state.WaitingForField is not null) //INPUT
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
                    state.resetDraft();
                    await _userStateRepository.SaveAsync(state, ct);
                    await RenderTaskCreationMenuAsync(chatId, messageId, state, ct);
                    break;
                case "edit_name":
                    // state.CurrentState = StateType.CreateTaskInput;
                    state.WaitingForField = "name";
                    await _userStateRepository.SaveAsync(state, ct);
                    // Редактируем сообщение, убираем кнопки, просим текст
                    await _bot.EditMessageText(chatId, messageId, "Введите название задачи:", cancellationToken: ct);
                    break;

                case "edit_desc":
                    // state.CurrentState = StateType.CreateTaskInput;
                    state.WaitingForField = "desc";
                    await _userStateRepository.SaveAsync(state, ct);
                    await _bot.EditMessageText(chatId, messageId, "Введите описание задачи:", cancellationToken: ct);
                    break;

                case "edit_reward":
                    //state.CurrentState = StateType.CreateTaskInput;
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
                    state.resetDraft();
                    await _userStateRepository.SaveAsync(state, ct);
                    // Возвращаем главное меню ( отредактировать это же сообщение, а перед этим отправить новое с уведомлением, что создание отменено)
                    var msg = await _bot.SendMessage(chatId, "Создание задачи отменено.", cancellationToken: ct);
                    await mainMenuRenderer.EditAsync(chatId, msg.Id, ct);
                    //  queue.Enqueue(StateType.MainMenu, update, state);

                    break;

                case "confirm_create":
                    var confirmed = await ConfirmCreating(state, callback, ct);
                    if (!confirmed) return;
                    break;
            }

            await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
        }

        private async Task<bool> ConfirmCreating(UserState state, CallbackQuery callback, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(state.DraftName) ||
                    string.IsNullOrWhiteSpace(state.DraftReward))
            {
                await _bot.AnswerCallbackQuery(callback.Id, "Заполните имя и награду!", showAlert: true, cancellationToken: ct);
                return false;
            }
            var userGuid = await _userService.GetOrCreateByTelegramIdAsync(state.TelegramUserId, ct);
            var createdTask = new TaskBuilder()
            .WithName(state.DraftName)
            .WithDescription(state.DraftDescription)
            .WithReward(state.DraftReward)
            .Build(userGuid.UserId);

            await _taskService.CreateTaskAsync(createdTask, ct);

            await _userStateRepository.SaveAsync(state, ct);
            await _bot.EditMessageText(callback.Message!.Chat.Id, callback.Message.Id, $"Задача '{state.DraftName}' создана!", cancellationToken: ct);
            //state.CurrentState = StateType.MainMenu;
            state.resetDraft();
            return true;
        }

    }

}




