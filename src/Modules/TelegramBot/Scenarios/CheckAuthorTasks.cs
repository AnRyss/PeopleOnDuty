using PeopleOnDuty.src.Modules.Shared;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios
{
    public class CheckAuthorTasks(IUserService userService, ITaskService taskService, ITelegramBotClient bot) : IScenario
    {
        public IEnumerable<string> HandledCallbacks => new[] { "author_task" };

        public bool CanHandleInput(string? waitingForField) => false;
        

        //  public bool CanHandle(StateType state, Update update) => state is StateType.AuthorCreatedTasks;



        public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
        {
            if (update.CallbackQuery is not { } callback) return;

            var chatId = callback.Message!.Chat.Id;
            var messageId = callback.Message.MessageId;
            var data = callback.Data;
            await RenderAuthorTasks(chatId, state, ct);
        }
        public async Task RenderAuthorTasks(long chatId, UserState state, CancellationToken ct)
        {
            var user = await userService.GetOrCreateByTelegramIdAsync(state.TelegramUserId, ct);

            var authorTasks = await taskService.GetByAuthorAsync(user.UserId, ct);



            var buttons = authorTasks
                .Select(t => new[]
                {
            InlineKeyboardButton.WithCallbackData(
                $"{t.Name} — {t.Reward}",
                $"author_task:{t.TaskId}")
                })
                .ToArray();

            var keyboard = new InlineKeyboardMarkup(buttons);

            await bot.SendMessage(
                chatId,
                "Выберите задачу, которую хотите посмотреть:",
                replyMarkup: keyboard,
                cancellationToken: ct);
        }
    }
}